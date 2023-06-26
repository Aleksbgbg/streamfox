package controllers

import (
	"bytes"
	"fmt"
	"unicode"

	"github.com/go-playground/validator/v10"
)

func addSpaces(str string) string {
	buffer := &bytes.Buffer{}
	for i, rune := range str {
		if unicode.IsUpper(rune) && i > 0 {
			buffer.WriteRune(' ')
		}
		buffer.WriteRune(rune)
	}
	return buffer.String()
}

func prettyFormat(err validator.FieldError) string {
	fieldName := addSpaces(err.Field())

	switch err.Tag() {
	case "required":
		return fmt.Sprintf("%s must be provided.", fieldName)
	case "min":
		return fmt.Sprintf(
			"%s must be at least %s characters long (currently %d).",
			fieldName,
			err.Param(),
			len(err.Value().(string)),
		)
	case "max":
		return fmt.Sprintf(
			"%s must be at most %s characters long (currently %d).",
			fieldName,
			err.Param(),
			len(err.Value().(string)),
		)
	case "email":
		return fmt.Sprintf("%s must be a valid email address.", fieldName)
	case "eqfield":
		return fmt.Sprintf("%s must be identical to %s.", fieldName, addSpaces(err.Param()))
	case "printascii":
		return fmt.Sprintf("%s must contain valid characters (printable ASCII only).", fieldName)
	default:
		return err.Error()
	}
}

type errorMap = map[string][]string

func formatErrors(err error) any {
	if _, ok := err.(validator.ValidationErrors); !ok {
		return err.Error()
	}

	errors := make(errorMap)

	for _, value := range err.(validator.ValidationErrors) {
		if _, found := errors[value.Field()]; !found {
			errors[value.Field()] = []string{}
		}

		errors[value.Field()] = append(errors[value.Field()], prettyFormat(value))
	}

	return errors
}
