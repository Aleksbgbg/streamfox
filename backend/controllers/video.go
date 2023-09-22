package controllers

import (
	"bytes"
	"errors"
	"fmt"
	"image"
	"image/draw"
	"image/jpeg"
	"image/png"
	"io"
	"net/http"
	"os"
	"streamfox-backend/codec"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"time"

	"github.com/gin-gonic/gin"
)

type CreateVideoInfo struct {
	Name string `json:"name" binding:"max=256"`
}

type VideoCreatedInfo struct {
	Id          string            `json:"id"`
	Name        string            `json:"name"`
	Description string            `json:"description"`
	Visibility  models.Visibility `json:"visibility"`
}

func CreateVideo(c *gin.Context) {
	var info CreateVideoInfo

	if ok := checkValidationError(c, c.ShouldBindJSON(&info)); !ok {
		return
	}

	user := getUserParam(c)

	video, err := models.NewVideo(user, info.Name)

	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	c.JSON(http.StatusCreated, VideoCreatedInfo{
		Id:          video.Id.String(),
		Name:        video.Name,
		Description: video.Description,
		Visibility:  video.Visibility,
	})
}

const VIDEO_PARAM_KEY = "video"

func ExtractVideoMiddleware(c *gin.Context) {
	videoId, err := models.IdFromString(c.Param("id"))

	if ok := checkUserError(c, err, errVideoInvalidId); !ok {
		return
	}

	video, err := models.FetchVideo(videoId)

	if ok := checkUserError(c, err, errVideoIdNonExistent); !ok {
		return
	}

	c.Set(VIDEO_PARAM_KEY, video)
}

func getVideoParam(c *gin.Context) *models.Video {
	return c.MustGet(VIDEO_PARAM_KEY).(*models.Video)
}

func EnsureCompleteVideoMiddleware(c *gin.Context) {
	video := getVideoParam(c)

	if video.Status < models.COMPLETE {
		userError(c, errVideoUploadIncomplete)
	}
}

func EnsureVisibleVideoMiddleware(c *gin.Context) {
	video := getVideoParam(c)

	if video.Visibility == models.PRIVATE {
		if !hasUserParam(c) {
			userError(c, errUserRequired)
			return
		}

		if !video.IsCreator(getUserParam(c)) {
			userError(c, errGenericAccessForbidden)
			return
		}
	}
}

func EnsureIsOwnerMiddleware(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	if !video.IsCreator(user) {
		userError(c, errVideoNotOwned)
	}
}

type VideoUpdateInfo struct {
	Name        string             `json:"name"        binding:"required,min=2,max=256"`
	Description *string            `json:"description" binding:"required"`
	Visibility  *models.Visibility `json:"visibility"  binding:"required,min=0,max=2"`
}

func UpdateVideo(c *gin.Context) {
	var update VideoUpdateInfo

	if ok := checkValidationError(c, c.ShouldBindJSON(&update)); !ok {
		return
	}

	video := getVideoParam(c)
	video.Name = update.Name
	video.Description = *update.Description
	video.Visibility = *update.Visibility
	err := video.Save()

	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	c.Status(http.StatusNoContent)
}

func UploadVideo(c *gin.Context) {
	video := getVideoParam(c)

	if video.Status > models.UPLOADING {
		userError(c, errVideoCannotOverwrite)
		return
	}

	video.Status = models.UPLOADING
	defer video.Save()

	file, filepath, err := files.VideoHandle(files.Stream, video.Id)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	_, err = io.Copy(file, c.Request.Body)

	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		file.Close()
		os.Remove(filepath)
		return
	}

	err = file.Close()

	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		os.Remove(filepath)
		return
	}

	probe, err := codec.Probe(video.Id)

	if err != nil {
		os.Remove(filepath)

		if errors.Is(err, codec.ErrInvalidVideoType) {
			userError(c, errVideoInvalidFormat)
		} else {
			serverError(c, err, errVideoProbe)
		}
		return
	}

	info, err := os.Stat(filepath)

	if ok := checkServerError(c, err, errVideoGetSize); !ok {
		return
	}

	video.MimeType = probe.MimeType
	video.DurationSecs = probe.DurationSecs
	video.SizeBytes = info.Size()
	video.Status = models.PROCESSING

	err = codec.GenerateThumbnail(video.Id, probe)

	if ok := checkServerError(c, err, errVideoGenerateThumbnail); !ok {
		return
	}

	video.Status = models.COMPLETE

	c.Status(http.StatusNoContent)
}

type VideoInfo struct {
	Id           string            `json:"id"`
	Creator      UserInfo          `json:"creator"`
	DurationSecs int32             `json:"durationSecs"`
	UploadedAt   time.Time         `json:"uploadedAt"`
	Name         string            `json:"name"`
	Description  string            `json:"description"`
	Visibility   models.Visibility `json:"visibility"`
	Views        int64             `json:"views"`
	Likes        int64             `json:"likes"`
	Dislikes     int64             `json:"dislikes"`
}

func getVideoInfo(video *models.Video) (*VideoInfo, error) {
	views, err := models.CountViews(video)

	if err != nil {
		return nil, err
	}

	return &VideoInfo{
		Id:           video.Id.String(),
		Creator:      getUserInfo(&video.Creator),
		DurationSecs: video.DurationSecs,
		UploadedAt:   video.CreatedAt,
		Name:         video.Name,
		Description:  video.Description,
		Visibility:   video.Visibility,
		Views:        views,
		Likes:        0,
		Dislikes:     0,
	}, nil
}

func GetVideos(c *gin.Context) {
	videos, err := models.FetchAllVideos()

	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	videoInfos := make([]*VideoInfo, 0)
	for _, video := range videos {
		videoInfo, err := getVideoInfo(&video)

		if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
			return
		}

		videoInfos = append(videoInfos, videoInfo)
	}

	c.JSON(http.StatusOK, videoInfos)
}

func GetVideoInfo(c *gin.Context) {
	video := getVideoParam(c)

	videoInfo, err := getVideoInfo(video)

	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	c.JSON(http.StatusOK, videoInfo)
}

func GetVideoThumbnail(c *gin.Context) {
	c.File(files.VideoPath(files.Thumbnail, getVideoParam(c).Id))
}

func loadImage(decoder func(io.Reader) (image.Image, error), path string) (image.Image, error) {
	reader, err := os.Open(path)

	if err != nil {
		return nil, err
	}

	defer reader.Close()

	return decoder(reader)
}

func GetVideoPreview(c *gin.Context) {
	video := getVideoParam(c)

	thumbnail, err := loadImage(
		jpeg.Decode,
		files.VideoPath(files.Thumbnail, video.Id),
	)

	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	logo, err := loadImage(png.Decode, "logo_preview.png")

	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	rect := thumbnail.Bounds()
	preview := image.NewRGBA(rect)
	for x := rect.Min.X; x <= rect.Max.X; x++ {
		for y := rect.Min.Y; y <= rect.Max.Y; y++ {
			preview.Set(x, y, thumbnail.At(x, y))
		}
	}

	drawStart := image.Pt(thumbnail.Bounds().Dx()-logo.Bounds().Dx()-10, 10)
	draw.Draw(
		preview,
		image.Rectangle{drawStart, drawStart.Add(logo.Bounds().Size())},
		logo,
		logo.Bounds().Min,
		draw.Over,
	)

	buf := new(bytes.Buffer)
	err = jpeg.Encode(buf, preview, nil)

	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	c.Data(http.StatusOK, "image/jpeg", buf.Bytes())
}

func GetVideoStream(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	c.File(files.VideoPath(files.Stream, video.Id))
	c.Header("Content-Type", video.MimeType)

	bytesStreamed := int64(c.Writer.Size())

	if bytesStreamed <= 0 {
		return
	}

	err := video.ProcessStream(user, bytesStreamed)

	recordError(c, err)
}

type WatchConditionsResponse struct {
	Percentage      float64 `json:"percentage"`
	RemainingBytes  uint64  `json:"remainingBytes"`
	RemainingTimeMs uint64  `json:"remainingTimeMs"`
}

func GetWatchConditions(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	conditions, err := video.CalculateWatchConditions(user)

	if ok := checkServerError(c, err, errVideoGetWatchConditions); !ok {
		return
	}

	c.JSON(http.StatusOK, WatchConditionsResponse{
		Percentage:      models.WatchPercentageRequired,
		RemainingBytes:  conditions.RemainingBytes,
		RemainingTimeMs: uint64(conditions.RemainingTime.Milliseconds()),
	})
}

func PostView(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	result, err := video.TryAddView(user)

	if ok := checkServerError(c, err, errVideoProcessStillWatching); !ok {
		return
	}

	switch result.Code {
	case models.ADD_VIEW_SUCCESS:
		c.Status(http.StatusNoContent)
	case models.ADD_VIEW_DUPLICATE:
		userError(c, errVideoViewAlreadyCounted)
	case models.ADD_VIEW_VIDEO_NOT_STREAMED_ENOUGH:
		validationErrorGeneric(
			c,
			fmt.Sprintf("You need to stream another %d bytes.", result.Conditions.RemainingBytes),
		)
	case models.ADD_VIEW_TIME_NOT_PASSED:
		validationErrorGeneric(
			c,
			fmt.Sprintf(
				"You need to watch another %dms.",
				result.Conditions.RemainingTime.Milliseconds(),
			),
		)
	}
}
