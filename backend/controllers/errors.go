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
	AUTHENTICATION_ERROR
	FORBIDDEN_ERROR
	NOT_FOUND_ERROR
)

func toHttpError(errorType ErrorType) (e int) {
	switch errorType {
	case SERVER_ERROR:
		return http.StatusInternalServerError
	case VALIDATION_ERROR:
		return http.StatusBadRequest
	case AUTHENTICATION_ERROR:
		return http.StatusUnauthorized
	case FORBIDDEN_ERROR:
		return http.StatusForbidden
	case NOT_FOUND_ERROR:
		return http.StatusNotFound
	}

	log.Panicf("toHttpError failed because errorType %d is not handled", errorType)
	return
}

func errorMessage(c *gin.Context, errorType ErrorType, message any) {
	c.JSON(toHttpError(errorType), gin.H{"errors": message})
}

type PredefinedError int

const (
	USER_REQUIRED PredefinedError = iota
	DATABASE_READ_FAILED
	DATABASE_WRITE_FAILED
	DATA_CREATION_FAILED
	FILE_IO_FAILED
	VIDEO_ID_INVALID
	VIDEO_ID_NON_EXISTENT
	VIDEO_NOT_OWNED
	VIDEO_UPLOAD_INCOMPLETE
	ACCESS_FORBIDDEN
	USER_MERGE_FAILED
)

func getPredefinedError(predefinedError PredefinedError) (e ErrorType, s string) {
	switch predefinedError {
	case USER_REQUIRED:
		return AUTHENTICATION_ERROR, "No user was logged in but a user is required."
	case DATABASE_READ_FAILED:
		return SERVER_ERROR, "Could not read from database."
	case DATABASE_WRITE_FAILED:
		return SERVER_ERROR, "Could not write to database."
	case DATA_CREATION_FAILED:
		return SERVER_ERROR, "Could not create data files."
	case FILE_IO_FAILED:
		return SERVER_ERROR, "Could not write to file."
	case VIDEO_ID_INVALID:
		return VALIDATION_ERROR, "Video ID is invalid."
	case VIDEO_ID_NON_EXISTENT:
		return NOT_FOUND_ERROR, "Video does not exist."
	case VIDEO_NOT_OWNED:
		return FORBIDDEN_ERROR, "Cannot make modifications to a video you do not own."
	case VIDEO_UPLOAD_INCOMPLETE:
		return FORBIDDEN_ERROR, "Video upload has not yet completed."
	case ACCESS_FORBIDDEN:
		return FORBIDDEN_ERROR, "You are not allowed access to this resource."
	case USER_MERGE_FAILED:
		return SERVER_ERROR, "Could not update authenticated user with anonymous statistics."
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
