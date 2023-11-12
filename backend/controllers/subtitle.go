package controllers

import (
	"io"
	"net/http"
	"streamfox-backend/codec"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"strings"

	"github.com/gin-gonic/gin"
)

const subtitleParamKey = "subtitle"

func ExtractSubtitleMiddleware(c *gin.Context) {
	subtitleId, err := models.IdFromString(c.Param("subtitle-id"))
	if ok := checkUserError(c, err, errVideoSubtitlesInvalidId); !ok {
		return
	}

	subtitle, err := models.GetSubtitle(subtitleId)
	if ok := checkUserError(c, err, errVideoSubtitlesIdNonExistent); !ok {
		return
	}

	c.Set(subtitleParamKey, subtitle)
}

func getSubtitleParam(c *gin.Context) *models.Subtitle {
	return c.MustGet(subtitleParamKey).(*models.Subtitle)
}

type SubtitleInfo struct {
	SubtitlesExtracted bool `json:"subtitlesExtracted"`
}

func GetSubtitlesInfo(c *gin.Context) {
	video := getVideoParam(c)

	c.JSON(http.StatusOK, SubtitleInfo{
		SubtitlesExtracted: video.SubtitlesExtracted,
	})
}

type Subtitle struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}

func getSubtitleInfo(subtitle *models.Subtitle) Subtitle {
	return Subtitle{
		Id:   subtitle.Id.String(),
		Name: subtitle.Name,
	}
}

func ExtractSubtitles(c *gin.Context) {
	video := getVideoParam(c)

	if video.SubtitlesExtracted {
		userError(c, errVideoSubtitlesCannotDoubleExtract)
		return
	}

	video.SubtitlesExtracted = true
	err := video.Save()
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	defer video.Save()
	video.SubtitlesExtracted = false

	subs, err := codec.ExtractAllSubtitles(video.Id)
	if ok := checkServerError(c, err, errVideoSubtitlesExtract); !ok {
		return
	}

	subtitles := []Subtitle{}
	for _, s := range subs {
		subtitle, err := models.CreateSubtitle(s.Id, video.Id, s.Title)
		if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
			return
		}

		subtitles = append(subtitles, getSubtitleInfo(subtitle))
	}

	video.SubtitlesExtracted = true

	c.JSON(http.StatusOK, subtitles)
}

func GetAllSubtitles(c *gin.Context) {
	video := getVideoParam(c)

	subs, err := models.GetAllSubtitles(video.Id)
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	subtitles := []Subtitle{}
	for _, s := range subs {
		subtitles = append(subtitles, getSubtitleInfo(&s))
	}
	c.JSON(http.StatusOK, subtitles)
}

func GetSubtitleContent(c *gin.Context) {
	video := getVideoParam(c)
	subtitle := getSubtitleParam(c)

	file, err := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		AddVar(files.SubtitleId, subtitle.Id).
		Resolve(files.VideoSubtitle)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	c.File(file.Path())
}

func storePlaceholderContents(vttSubtitleFile *files.File) error {
	const defaultSubtitleFile = `WEBVTT

00:00.000 --> 00:05.000
Welcome!
`

	vttHandle, err := vttSubtitleFile.Open()
	if err != nil {
		return err
	}

	_, err = io.Copy(vttHandle, strings.NewReader(defaultSubtitleFile))
	vttSubtitleFile.AutoClose()
	return err
}

func CreateSubtitle(c *gin.Context) {
	video := getVideoParam(c)

	subtitle, err := models.CreateSubtitle(models.NewId(), video.Id, "New Subtitle")
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	resolver := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		AddVar(files.SubtitleId, subtitle.Id)

	vttSubtitleFile, err := resolver.Resolve(files.VideoSubtitle)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		subtitle.Delete()
		return
	}

	success := false
	defer func() {
		if !success {
			subtitle.Delete()
			vttSubtitleFile.Remove()
		}
	}()

	err = storePlaceholderContents(vttSubtitleFile)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	if c.Request.ContentLength > 0 {
		subtitleContentsUpload, err := resolver.Resolve(files.VideoSubtitleTemp)
		if ok := checkServerError(c, err, errGenericFileIo); !ok {
			return
		}
		defer subtitleContentsUpload.Remove()

		srcHandle, err := subtitleContentsUpload.Open()
		if ok := checkServerError(c, err, errGenericFileIo); !ok {
			return
		}

		_, err = io.Copy(srcHandle, c.Request.Body)
		subtitleContentsUpload.AutoClose()
		if ok := checkServerError(c, err, errGenericFileIo); !ok {
			return
		}

		err = codec.ConvertToVtt(video.Id, subtitle.Id)
		if ok := checkUserError(c, err, errVideoSubtitlesInvalidFormat); !ok {
			return
		}
	}

	success = true

	c.JSON(http.StatusOK, getSubtitleInfo(subtitle))
}

type UpdateSubtitleInfo struct {
	Name    string `json:"name"    binding:"required,min=2,max=256"`
	Content string `json:"content" binding:"required"`
}

func UpdateSubtitle(c *gin.Context) {
	video := getVideoParam(c)
	subtitle := getSubtitleParam(c)

	var update UpdateSubtitleInfo
	if ok := checkValidationError(c, c.ShouldBindJSON(&update)); !ok {
		return
	}

	file, err := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		AddVar(files.SubtitleId, subtitle.Id).
		Resolve(files.VideoSubtitle)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	handle, err := file.Open()
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}
	defer file.AutoClose()

	_, err = io.Copy(handle, strings.NewReader(update.Content))
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	subtitle.Name = update.Name
	err = subtitle.Save()
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	c.Status(http.StatusNoContent)
}

func DeleteSubtitle(c *gin.Context) {
	video := getVideoParam(c)
	subtitle := getSubtitleParam(c)

	err := subtitle.Delete()
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	file, err := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		AddVar(files.SubtitleId, subtitle.Id).
		Resolve(files.VideoSubtitle)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}
	err = file.Remove()
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	c.Status(http.StatusNoContent)
}
