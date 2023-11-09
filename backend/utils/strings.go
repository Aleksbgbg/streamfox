package utils

import (
	"bytes"
	"unicode"
	"unicode/utf8"

	"github.com/dchest/uniuri"
)

func ToLowerCamelCase(str string) string {
	r, size := utf8.DecodeRuneInString(str)
	if r == utf8.RuneError && size <= 1 {
		return str
	}
	lowerCase := unicode.ToLower(r)
	if r == lowerCase {
		return str
	}
	return string(lowerCase) + str[size:]
}

func AddSpaces(str string) string {
	buffer := &bytes.Buffer{}
	for i, rune := range str {
		if unicode.IsUpper(rune) && i > 0 {
			buffer.WriteRune(' ')
		}
		buffer.WriteRune(rune)
	}
	return buffer.String()
}

func SecureString() string {
	return uniuri.New()
}
