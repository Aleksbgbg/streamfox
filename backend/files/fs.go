package files

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"strings"
)

var ErrPathKeyNotFound = errors.New("path not found")

const DefaultPerm = os.ModePerm

type VarMap = map[string]string

type Fs struct {
	keyToPath map[string]string
	vars      VarMap
}

func ParseFsTree(tree string) Fs {
	fs := Fs{
		keyToPath: map[string]string{},
		vars:      VarMap{},
	}

	var key []rune
	var path []rune
	pathSeparators := []int{0}
	lastIndentLevel := 0
	indentLevel := 0
	isKey := false
	isIndent := true

	for _, r := range fsTree {
		switch r {
		case '\t':
			indentLevel += 1
		case '\n':
			if len(path) == 0 {
				continue
			}

			if isKey {
				fs.keyToPath[string(key)] = string(path)

				key = nil
			} else {
				pathSeparators = append(pathSeparators, len(path))
				path = append(path, '/')
			}

			lastIndentLevel = indentLevel
			indentLevel = 0
			isKey = false
			isIndent = true
		case ' ':
			isKey = true
		default:
			if isIndent {
				isIndent = false

				if lastIndentLevel >= indentLevel {
					separator := pathSeparators[indentLevel]
					path = path[:separator]
					pathSeparators = pathSeparators[:indentLevel+1]
					if indentLevel != 0 {
						path = append(path, '/')
					}
				}
			}

			if isKey {
				if r != '#' {
					key = append(key, r)
				}
			} else {
				path = append(path, r)
			}
		}
	}

	return fs
}

func (fs *Fs) AddVar(key, val string) {
	fs.vars[key] = val
}

func resolveVar(key string, vars, additionalVars VarMap) (string, error) {
	if value, exists := vars[key]; exists {
		return value, nil
	} else if value, exists := additionalVars[key]; exists {
		return value, nil
	} else {
		return "", fmt.Errorf("no variable found with key '%s'", key)
	}
}

func (fs *Fs) ResolvePath(key string, additionalVars VarMap) (string, error) {
	builder := strings.Builder{}
	path, exists := fs.keyToPath[key]

	if !exists {
		return "", fmt.Errorf("key '%s' does not have an associated path: %w", key, ErrPathKeyNotFound)
	}

	varName := strings.Builder{}
	isVar := false

	for _, r := range path {
		switch r {
		case '<':
			isVar = true
		case '>':
			varValue, err := resolveVar(varName.String(), fs.vars, additionalVars)
			if err != nil {
				return "", err
			}

			builder.WriteString(varValue)

			varName.Reset()
			isVar = false
		default:
			if isVar {
				varName.WriteRune(r)
			} else {
				builder.WriteRune(r)
			}
		}
	}

	fullPath := builder.String()

	dir := filepath.Dir(fullPath)
	err := os.MkdirAll(dir, DefaultPerm)
	if err != nil {
		return "", err
	}

	handle, err := os.OpenFile(fullPath, os.O_CREATE|os.O_RDWR, DefaultPerm)
	if err != nil {
		return "", err
	}
	handle.Close()

	return fullPath, nil
}

func (fs *Fs) ResolveFile(key string, additionalVars VarMap) (*os.File, string, error) {
	fullPath, err := fs.ResolvePath(key, additionalVars)
	if err != nil {
		return nil, "", err
	}

	handle, err := os.OpenFile(fullPath, os.O_CREATE|os.O_RDWR, DefaultPerm)
	if err != nil {
		return nil, "", err
	}

	return handle, fullPath, nil
}
