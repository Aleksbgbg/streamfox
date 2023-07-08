package controllers

import (
	"fmt"
	"io"
	"net/http"
	"os"
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

func PostVideo(c *gin.Context) {
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

	dataRoot := os.Getenv("DATA_ROOT")
	videoId := snowflake.ParseInt64(video.Id)

	err = os.MkdirAll(fmt.Sprintf("%s/videos/%s", dataRoot, videoId.Base58()), os.ModePerm)

	if err != nil {
		errorPredefined(c, DATA_CREATION_FAILED)
		return
	}

	file, err := os.Create(fmt.Sprintf("%s/videos/%s/video", dataRoot, videoId.Base58()))
	if err != nil {
		errorPredefined(c, DATA_CREATION_FAILED)
		return
	}
	defer file.Close()

	_, err = io.Copy(file, c.Request.Body)

	if err != nil {
		errorPredefined(c, FILE_IO_FAILED)
		return
	}

	c.JSON(http.StatusCreated, VideoCreatedInfo{
		Id:          videoId.Base58(),
		Name:        video.Name,
		Description: video.Description,
		Visibility:  video.Visibility,
	})
}
