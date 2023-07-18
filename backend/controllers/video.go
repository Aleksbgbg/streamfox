package controllers

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"streamfox-backend/codec"
	"streamfox-backend/models"
	"streamfox-backend/utils"

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
	user := getUserParam(c)

	video, err := models.NewVideo(user)

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

const VIDEO_PARAM_KEY = "video"

func ExtractVideoMiddleware(c *gin.Context) {
	videoId, err := snowflake.ParseBase58([]byte(c.Param("id")))

	if err != nil {
		errorPredefined(c, VIDEO_ID_INVALID)
		c.Abort()
	}

	video, err := models.FetchVideo(videoId)

	if err != nil {
		errorPredefined(c, VIDEO_ID_NON_EXISTENT)
		c.Abort()
	}

	c.Set(VIDEO_PARAM_KEY, video)
}

func getVideoParam(c *gin.Context) *models.Video {
	return c.MustGet(VIDEO_PARAM_KEY).(*models.Video)
}

func EnsureCompleteVideoMiddleware(c *gin.Context) {
	video := getVideoParam(c)

	if video.Status < models.COMPLETE {
		errorPredefined(c, VIDEO_UPLOAD_INCOMPLETE)
		c.Abort()
	}
}

func EnsureVisibleVideoMiddleware(c *gin.Context) {
	video := getVideoParam(c)

	if video.Visibility == models.PRIVATE {
		if !hasUserParam(c) {
			errorPredefined(c, USER_REQUIRED)
			c.Abort()
			return
		}

		if !video.IsCreator(getUserParam(c)) {
			errorPredefined(c, ACCESS_FORBIDDEN)
			c.Abort()
			return
		}
	}
}

func EnsureIsOwnerMiddleware(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	if !video.IsCreator(user) {
		errorPredefined(c, VIDEO_NOT_OWNED)
		c.Abort()
	}
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

	video := getVideoParam(c)
	video.Name = update.Name
	video.Description = *update.Description
	video.Visibility = *update.Visibility
	err := video.Save()

	if err != nil {
		errorPredefined(c, DATABASE_WRITE_FAILED)
		return
	}

	c.Status(http.StatusNoContent)
}

func UploadVideo(c *gin.Context) {
	video := getVideoParam(c)

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

	dataRoot := utils.GetEnvVar(utils.DATA_ROOT)

	videoDir := fmt.Sprintf("%s/videos/%s", dataRoot, video.IdSnowflake().Base58())
	err := os.MkdirAll(videoDir, os.ModePerm)

	if err != nil {
		errorPredefined(c, DATA_CREATION_FAILED)
		return
	}

	filepath := fmt.Sprintf("%s/videos/%s/video", dataRoot, video.IdSnowflake().Base58())
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

	probe, err := codec.Probe(filepath)

	if err != nil {
		os.Remove(filepath)

		if _, ok := err.(*codec.InvalidVideoTypeError); ok {
			errorMessage(c, VALIDATION_ERROR, "Invalid video format.")
		} else {
			errorMessage(c, SERVER_ERROR, "Unable to probe video.")
		}
		return
	}

	video.MimeType = probe.MimeType
	video.DurationSecs = probe.DurationSecs
	video.Status = models.PROCESSING

	err = codec.GenerateThumbnail(videoDir)

	if err != nil {
		errorMessage(c, SERVER_ERROR, fmt.Sprintf("Error in generating thumbnail: %s.", err.Error()))
		return
	}

	video.Status = models.COMPLETE

	c.Status(http.StatusNoContent)
}

type VideoInfo struct {
	Id           string            `json:"id"`
	Creator      UserInfo          `json:"creator"`
	DurationSecs int32             `json:"duration_secs"`
	Name         string            `json:"name"`
	Description  string            `json:"description"`
	Visibility   models.Visibility `json:"visibility"`
	Views        int64             `json:"views"`
	Likes        int64             `json:"likes"`
	Dislikes     int64             `json:"dislikes"`
}

func getVideoInfo(video *models.Video) VideoInfo {
	return VideoInfo{
		Id:           video.IdSnowflake().Base58(),
		Creator:      getUserInfo(&video.Creator),
		DurationSecs: video.DurationSecs,
		Name:         video.Name,
		Description:  video.Description,
		Visibility:   video.Visibility,
		Views:        video.Views,
		Likes:        video.Likes,
		Dislikes:     video.Dislikes,
	}
}

func GetVideos(c *gin.Context) {
	videos, err := models.FetchAllVideos()

	if err != nil {
		errorPredefined(c, DATABASE_READ_FAILED)
		return
	}

	videoInfos := make([]VideoInfo, 0)
	for _, video := range videos {
		videoInfos = append(videoInfos, getVideoInfo(&video))
	}

	c.JSON(http.StatusOK, videoInfos)
}

func GetVideoInfo(c *gin.Context) {
	video := getVideoParam(c)
	c.JSON(http.StatusOK, getVideoInfo(video))
}

func GetVideoThumbnail(c *gin.Context) {
	video := getVideoParam(c)
	dataRoot := utils.GetEnvVar(utils.DATA_ROOT)
	filepath := fmt.Sprintf("%s/videos/%s/thumbnail", dataRoot, video.IdSnowflake().Base58())

	c.File(filepath)
}

func GetVideoStream(c *gin.Context) {
	video := getVideoParam(c)
	dataRoot := utils.GetEnvVar(utils.DATA_ROOT)
	filepath := fmt.Sprintf("%s/videos/%s/video", dataRoot, video.IdSnowflake().Base58())

	c.File(filepath)
	c.Header("Content-Type", video.MimeType)
}
