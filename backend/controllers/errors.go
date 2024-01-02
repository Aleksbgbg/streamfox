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

type errorMap map[string][]string

type errorResponse struct {
	Generic  []string `json:"generic"`
	Specific errorMap `json:"specific"`
}

func genericError(message string) errorResponse {
	return errorResponse{
		Generic:  []string{message},
		Specific: errorMap{},
	}
}

func specificErrors(specific errorMap) errorResponse {
	return errorResponse{
		Generic:  []string{},
		Specific: specific,
	}
}

func errorMessage(c *gin.Context, errorType errType, message errorResponse) {
	c.JSON(toHttpError(errorType), gin.H{"errors": message})
}

type predefinedError int

const (
	errGenericInvalidContentRange predefinedError = iota
	errAuthInvalidCredentials
	errUserInvalidId
	errVideoInvalidId
	errVideoSubtitlesInvalidId
	errVideoInvalidFormat
	errVideoCannotOverwrite
	errVideoSubtitlesCannotDoubleExtract
	errVideoSubtitlesInvalidFormat

	errUserRequired

	errGenericAccessForbidden
	errVideoNotOwned
	errVideoUploadIncomplete

	errUserIdNonExistent
	errVideoIdNonExistent
	errVideoSubtitlesIdNonExistent

	errGenericDatabaseIo
	errGenericFileIo
	errAuthGeneratingToken
	errAuthGeneratingUser
	errUserMergeFailed
	errVideoProbe
	errVideoGenerateThumbnail
	errVideoViewProcessWatchHint
	errVideoSubtitlesExtract
)

func getPredefinedError(predefined predefinedError) (errType, string) {
	switch predefined {
	case errGenericInvalidContentRange:
		return errValidation, "Invalid Content-Range header."
	case errAuthInvalidCredentials:
		return errValidation, "Invalid credentials."
	case errUserInvalidId:
		return errValidation, "User ID is invalid."
	case errVideoInvalidId:
		return errValidation, "Video ID is invalid."
	case errVideoSubtitlesInvalidId:
		return errValidation, "Subtitle ID is invalid."
	case errVideoInvalidFormat:
		return errValidation, "Invalid video format."
	case errVideoCannotOverwrite:
		return errValidation, "Cannot overwrite video after uploading has completed successfully."
	case errVideoSubtitlesCannotDoubleExtract:
		return errValidation, "Video subtitles have already been extracted."
	case errVideoSubtitlesInvalidFormat:
		return errValidation, "Provided file cannot be converted to WebVTT (web subtitles)."

	case errUserRequired:
		return errAuthentication, "No user was logged in but a user is required."

	case errGenericAccessForbidden:
		return errForbidden, "You are not allowed access to this resource."
	case errVideoNotOwned:
		return errForbidden, "Cannot make modifications to a video you do not own."
	case errVideoUploadIncomplete:
		return errForbidden, "Video upload has not yet completed."

	case errUserIdNonExistent:
		return errNotFound, "User does not exist."
	case errVideoIdNonExistent:
		return errNotFound, "Video does not exist."
	case errVideoSubtitlesIdNonExistent:
		return errNotFound, "Subtitle does not exist."

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
	case errVideoGenerateThumbnail:
		return errServer, "Error in generating thumbnail."
	case errVideoViewProcessWatchHint:
		return errServer, "Could not process watch hint."
	case errVideoSubtitlesExtract:
		return errServer, "Error in extracting subtitles."
	}

	log.Panicf("getPredefinedError failed because predefinedError %d is not handled", predefined)
	return 0, ""
}

func validationError(c *gin.Context, errors errorMap) {
	errorMessage(c, errValidation, specificErrors(errors))
	c.Abort()
}

func userError(c *gin.Context, predefined predefinedError) {
	errorType, message := getPredefinedError(predefined)
	errorMessage(c, errorType, genericError(message))
	c.Abort()
}

func serverError(c *gin.Context, err error, predefined predefinedError) {
	errorType, message := getPredefinedError(predefined)
	c.Error(fmt.Errorf("'%s': %v", message, err))
	errorMessage(c, errorType, genericError(message))
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
	errorMessage(c, errorType, genericError(message))
	c.Abort()
	return false
}

func checkServerError(c *gin.Context, err error, predefined predefinedError) bool {
	if err == nil {
		return true
	}

	errorType, message := getPredefinedError(predefined)
	c.Error(fmt.Errorf("'%s': %v", message, err))
	errorMessage(c, errorType, genericError(message))
	c.Abort()
	return false
}

func formatErrors(err error) errorResponse {
	if _, ok := err.(validator.ValidationErrors); !ok {
		return genericError(err.Error())
	}

	errors := errorMap{}

	for _, value := range err.(validator.ValidationErrors) {
		name := utils.ToLowerCamelCase(value.Field())

		if _, found := errors[value.Field()]; !found {
			errors[name] = []string{}
		}

		errors[name] = append(errors[name], prettyFormat(value))
	}

	return specificErrors(errors)
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
