package controllers

import (
	"fmt"
	"streamfox-backend/utils"

	"github.com/go-playground/validator/v10"
)

func prettyFormat(err validator.FieldError) string {
	fieldName := utils.AddSpaces(err.Field())

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
		return fmt.Sprintf("%s must be identical to %s.", fieldName, utils.AddSpaces(err.Param()))
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
		name := utils.ToLowerCamelCase(value.Field())

		if _, found := errors[value.Field()]; !found {
			errors[name] = []string{}
		}

		errors[name] = append(errors[name], prettyFormat(value))
	}

	return errors
}
