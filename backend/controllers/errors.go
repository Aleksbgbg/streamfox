package controllers

import (
	"fmt"
	"log"
	"net/http"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator/v10"
)

type ErrorType int

const (
	SERVER_ERROR ErrorType = iota
	VALIDATION_ERROR
	AUTHORIZATION_ERROR
)

func toHttpError(errorType ErrorType) (e int) {
	switch errorType {
	case SERVER_ERROR:
		return http.StatusInternalServerError
	case VALIDATION_ERROR:
		return http.StatusBadRequest
	case AUTHORIZATION_ERROR:
		return http.StatusUnauthorized
	}

	log.Panicf("toHttpError failed because errorType %d is not handled", errorType)
	return
}

func errorMessage(c *gin.Context, errorType ErrorType, message any) {
	c.JSON(toHttpError(errorType), gin.H{"errors": message})
}

type PredefinedError int

const (
	USER_FETCH_FAILED PredefinedError = iota
	DATABASE_WRITE_FAILED
	DATA_CREATION_FAILED
	FILE_IO_FAILED
)

func getPredefinedError(predefinedError PredefinedError) (e ErrorType, s string) {
	switch predefinedError {
	case USER_FETCH_FAILED:
		return SERVER_ERROR, "Could not fetch current user."
	case DATABASE_WRITE_FAILED:
		return SERVER_ERROR, "Could not write to database."
	case DATA_CREATION_FAILED:
		return SERVER_ERROR, "Could not create data files."
	case FILE_IO_FAILED:
		return SERVER_ERROR, "Could not write to file."
	}

	log.Panicf("getPredefinedError failed because predefinedError %d is not handled", predefinedError)
	return
}

func errorPredefined(c *gin.Context, predefinedError PredefinedError) {
	errorType, message := getPredefinedError(predefinedError)
	errorMessage(c, errorType, message)
}

func prettyFormat(err validator.FieldError) string {
	fieldName := utils.AddSpaces(err.Field())

	switch err.Tag() {
	case "required":
		return fmt.Sprintf("%s must be provided.", fieldName)
	case "email":
		return fmt.Sprintf("%s must be a valid email address.", fieldName)
	case "eqfield":
		return fmt.Sprintf("%s must be identical to %s.", fieldName, utils.AddSpaces(err.Param()))
	case "printascii":
		return fmt.Sprintf("%s must contain valid characters (printable ASCII only).", fieldName)
	case "min", "max":
		value := err.Value()

		if str, ok := value.(string); ok {
			var modifier string

			if err.Tag() == "min" {
				modifier = "least"
			} else {
				modifier = "most"
			}

			return fmt.Sprintf(
				"%s must be at %s %s characters long (currently %d).",
				fieldName,
				modifier,
				err.Param(),
				len(str),
			)
		} else {
			var modifier string

			if err.Tag() == "min" {
				modifier = "lower"
			} else {
				modifier = "higher"
			}

			return fmt.Sprintf("%s must not be %s than %s.", fieldName, modifier, err.Param())
		}
	}

	return err.Error()
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
