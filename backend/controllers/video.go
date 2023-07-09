package controllers

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"streamfox-backend/models"
	"streamfox-backend/utils"
	"streamfox-backend/utils/ffmpeg"

	"github.com/bwmarrin/snowflake"
	"github.com/gin-gonic/gin"
)

type VideoCreatedInfo struct {
	Id          string            `json:"id"`
	Name        string            `json:"name"`
	Description string            `json:"description"`
	Visibility  models.Visibility `json:"visibility"`
}

func CreateVideo(c *gin.Context) {
	userId, err := utils.ExtractUserId(c)

	if err != nil {
		errorPredefined(c, USER_FETCH_FAILED)
		return
	}

	video, err := models.NewVideo(userId)

	if err != nil {
		errorPredefined(c, DATABASE_WRITE_FAILED)
		return
	}

	c.JSON(http.StatusCreated, VideoCreatedInfo{
		Id:          video.IdSnowflake().Base58(),
		Name:        video.Name,
		Description: video.Description,
		Visibility:  video.Visibility,
	})
}

type VideoUpdateInfo struct {
	Name        string             `json:"name"        binding:"required,min=2,max=256"`
	Description *string            `json:"description" binding:"required"`
	Visibility  *models.Visibility `json:"visibility"  binding:"required,min=0,max=2"`
}

func UpdateVideo(c *gin.Context) {
	var update VideoUpdateInfo

	if err := c.ShouldBindJSON(&update); err != nil {
		errorMessage(c, VALIDATION_ERROR, formatErrors(err))
		return
	}

	videoId, err := snowflake.ParseBase58([]byte(c.Param("id")))

	if err != nil {
		errorPredefined(c, VIDEO_ID_INVALID)
		return
	}

	video, err := models.FetchVideo(videoId)

	if err != nil {
		errorPredefined(c, VIDEO_ID_NON_EXISTENT)
		return
	}

	userId, err := utils.ExtractUserId(c)

	if err != nil {
		errorPredefined(c, USER_FETCH_FAILED)
		return
	}

	if !video.IsCreator(userId) {
		errorPredefined(c, VIDEO_NOT_OWNED)
		return
	}

	video.Name = update.Name
	video.Description = *update.Description
	video.Visibility = *update.Visibility
	err = video.Save()

	if err != nil {
		errorPredefined(c, DATABASE_WRITE_FAILED)
		return
	}

	c.Status(http.StatusNoContent)
}

func UploadVideo(c *gin.Context) {
	videoId, err := snowflake.ParseBase58([]byte(c.Param("id")))

	if err != nil {
		errorPredefined(c, VIDEO_ID_INVALID)
		return
	}

	video, err := models.FetchVideo(videoId)

	if err != nil {
		errorPredefined(c, VIDEO_ID_NON_EXISTENT)
		return
	}

	userId, err := utils.ExtractUserId(c)

	if err != nil {
		errorPredefined(c, USER_FETCH_FAILED)
		return
	}

	if !video.IsCreator(userId) {
		errorPredefined(c, VIDEO_NOT_OWNED)
		return
	}

	if video.Status > models.UPLOADING {
		errorMessage(
			c,
			VALIDATION_ERROR,
			"Cannot rewrite video after uploading has completed successfully.",
		)
		return
	}

	video.Status = models.UPLOADING
	defer video.Save()

	dataRoot := os.Getenv("DATA_ROOT")

	videoDir := fmt.Sprintf("%s/videos/%s", dataRoot, videoId.Base58())
	err = os.MkdirAll(videoDir, os.ModePerm)

	if err != nil {
		errorPredefined(c, DATA_CREATION_FAILED)
		return
	}

	filepath := fmt.Sprintf("%s/videos/%s/video", dataRoot, videoId.Base58())
	file, err := os.Create(filepath)
	if err != nil {
		errorPredefined(c, DATA_CREATION_FAILED)
		return
	}

	_, err = io.Copy(file, c.Request.Body)
	file.Close()

	if err != nil {
		os.Remove(filepath)

		errorPredefined(c, FILE_IO_FAILED)
		return
	}

	probe, err := ffmpeg.Probe(filepath)

	if err != nil {
		os.Remove(filepath)

		if _, ok := err.(*ffmpeg.InvalidVideoTypeError); ok {
			errorMessage(c, VALIDATION_ERROR, "Invalid video format.")
		} else {
			errorMessage(c, SERVER_ERROR, "Unable to probe video.")
		}
		return
	}

	video.MimeType = probe.MimeType
	video.DurationSecs = probe.DurationSecs
	video.Status = models.PROCESSING

	err = ffmpeg.GenerateThumbnail(videoDir)

	if err != nil {
		errorMessage(c, SERVER_ERROR, fmt.Sprintf("Error in generating thumbnail: %s.", err.Error()))
		return
	}

	video.Status = models.COMPLETE

	c.Status(http.StatusNoContent)
}

func GetVideoThumbnail(c *gin.Context) {
	videoId, err := snowflake.ParseBase58([]byte(c.Param("id")))

	if err != nil {
		errorPredefined(c, VIDEO_ID_INVALID)
		return
	}

	video, err := models.FetchVideo(videoId)

	if err != nil {
		errorPredefined(c, VIDEO_ID_NON_EXISTENT)
		return
	}

	if video.Status < models.COMPLETE {
		errorPredefined(c, VIDEO_UPLOAD_INCOMPLETE)
		return
	}

	if video.Visibility == models.PRIVATE {
		userId, err := utils.ExtractUserId(c)

		if err != nil {
			errorPredefined(c, USER_FETCH_FAILED)
			return
		}

		if !video.IsCreator(userId) {
			errorPredefined(c, VIDEO_NOT_OWNED)
			return
		}
	}

	dataRoot := os.Getenv("DATA_ROOT")
	filepath := fmt.Sprintf("%s/videos/%s/thumbnail", dataRoot, videoId.Base58())

	c.File(filepath)
}

func GetVideoStream(c *gin.Context) {
	videoId, err := snowflake.ParseBase58([]byte(c.Param("id")))

	if err != nil {
		errorPredefined(c, VIDEO_ID_INVALID)
		return
	}

	video, err := models.FetchVideo(videoId)

	if err != nil {
		errorPredefined(c, VIDEO_ID_NON_EXISTENT)
		return
	}

	if video.Status < models.COMPLETE {
		errorPredefined(c, VIDEO_UPLOAD_INCOMPLETE)
		return
	}

	if video.Visibility == models.PRIVATE {
		userId, err := utils.ExtractUserId(c)

		if err != nil {
			errorPredefined(c, USER_FETCH_FAILED)
			return
		}

		if !video.IsCreator(userId) {
			errorPredefined(c, VIDEO_NOT_OWNED)
			return
		}
	}

	dataRoot := os.Getenv("DATA_ROOT")
	filepath := fmt.Sprintf("%s/videos/%s/video", dataRoot, videoId.Base58())

	c.File(filepath)
	c.Header("Content-Type", video.MimeType)
}
