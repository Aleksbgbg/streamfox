package controllers

import (
	"fmt"
	"log"
	"net/http"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
	"github.com/go-playground/validator/v10"
)

type errType int

const (
	errValidation errType = iota
	errAuthentication
	errForbidden
	errNotFound
	errServer
)

func toHttpError(e errType) int {
	switch e {
	case errValidation:
		return http.StatusBadRequest
	case errAuthentication:
		return http.StatusUnauthorized
	case errForbidden:
		return http.StatusForbidden
	case errNotFound:
		return http.StatusNotFound
	case errServer:
		return http.StatusInternalServerError
	}

	log.Panicf("toHttpError failed because errorType %d is not handled", e)
	return 0
}

func errorMessage(c *gin.Context, errorType errType, message any) {
	c.JSON(toHttpError(errorType), gin.H{"errors": message})
}

type predefinedError int

const (
	errAuthInvalidCredentials predefinedError = iota
	errVideoInvalidId
	errVideoInvalidFormat
	errVideoCannotOverwrite
	errVideoViewAlreadyCounted

	errUserRequired

	errGenericAccessForbidden
	errVideoNotOwned
	errVideoUploadIncomplete

	errVideoIdNonExistent

	errGenericDatabaseIo
	errGenericFileIo
	errAuthGeneratingToken
	errAuthGeneratingUser
	errUserMergeFailed
	errVideoProbe
	errVideoGetSize
	errVideoGenerateThumbnail
	errVideoGetWatchTime
	errVideoProcessStillWatching
)

func getPredefinedError(predefined predefinedError) (errType, string) {
	switch predefined {
	case errAuthInvalidCredentials:
		return errValidation, "Invalid credentials."
	case errVideoInvalidId:
		return errValidation, "Video ID is invalid."
	case errVideoInvalidFormat:
		return errValidation, "Invalid video format."
	case errVideoCannotOverwrite:
		return errValidation, "Cannot overwrite video after uploading has completed successfully."
	case errVideoViewAlreadyCounted:
		return errValidation, "View has already been counted."

	case errUserRequired:
		return errAuthentication, "No user was logged in but a user is required."

	case errGenericAccessForbidden:
		return errForbidden, "You are not allowed access to this resource."
	case errVideoNotOwned:
		return errForbidden, "Cannot make modifications to a video you do not own."
	case errVideoUploadIncomplete:
		return errForbidden, "Video upload has not yet completed."

	case errVideoIdNonExistent:
		return errNotFound, "Video does not exist."

	case errGenericDatabaseIo:
		return errServer, "Database transaction failed."
	case errGenericFileIo:
		return errServer, "File input / output failed."
	case errAuthGeneratingToken:
		return errServer, "Error in generating token."
	case errAuthGeneratingUser:
		return errServer, "Error in generating user."
	case errUserMergeFailed:
		return errServer, "Could not update authenticated user with anonymous statistics."
	case errVideoProbe:
		return errServer, "Unable to probe video."
	case errVideoGetSize:
		return errServer, "Could not get video size."
	case errVideoGenerateThumbnail:
		return errServer, "Error in generating thumbnail."
	case errVideoGetWatchTime:
		return errServer, "Could not get required watch time."
	case errVideoProcessStillWatching:
		return errServer, "Could not process still watching request."
	}

	log.Panicf("getPredefinedError failed because predefinedError %d is not handled", predefined)
	return 0, ""
}

func validationError(c *gin.Context, errors any) {
	errorMessage(c, errValidation, errors)
	c.Abort()
}

func userError(c *gin.Context, predefined predefinedError) {
	errorType, message := getPredefinedError(predefined)
	errorMessage(c, errorType, message)
	c.Abort()
}

func serverError(c *gin.Context, err error, predefined predefinedError) {
	errorType, message := getPredefinedError(predefined)
	c.Error(fmt.Errorf("'%s': %v", message, err))
	errorMessage(c, errorType, message)
	c.Abort()
}

func recordError(c *gin.Context, err error) bool {
	if err == nil {
		return true
	}

	c.Error(err)
	return false
}

func checkValidationError(c *gin.Context, err error) bool {
	if err == nil {
		return true
	}

	errorMessage(c, errValidation, formatErrors(err))
	c.Abort()
	return false
}

func checkUserError(c *gin.Context, err error, predefined predefinedError) bool {
	if err == nil {
		return true
	}

	errorType, message := getPredefinedError(predefined)
	errorMessage(c, errorType, message)
	c.Abort()
	return false
}

func checkServerError(c *gin.Context, err error, predefined predefinedError) bool {
	if err == nil {
		return true
	}

	errorType, message := getPredefinedError(predefined)
	c.Error(fmt.Errorf("'%s': %v", message, err))
	errorMessage(c, errorType, message)
	c.Abort()
	return false
}

func formatErrors(err error) any {
	if _, ok := err.(validator.ValidationErrors); !ok {
		return err.Error()
	}

	errors := map[string][]string{}

	for _, value := range err.(validator.ValidationErrors) {
		name := utils.ToLowerCamelCase(value.Field())

		if _, found := errors[value.Field()]; !found {
			errors[name] = []string{}
		}

		errors[name] = append(errors[name], prettyFormat(value))
	}

	return errors
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
