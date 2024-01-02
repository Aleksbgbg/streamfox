package controllers

import (
	"bytes"
	"errors"
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
	"github.com/ldez/mimetype"
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

	contentRange, err := parseContentRange(c)
	if ok := checkServerError(c, err, errGenericInvalidContentRange); !ok {
		return
	}

	if hasContentLength(c) && (contentRange.rangeSizeBytes != c.Request.ContentLength) {
		userError(c, errGenericInvalidContentRange)
		return
	}

	if contentRange.startByte == 0 {
		video.SizeBytes = contentRange.contentSizeBytes
	} else if contentRange.contentSizeBytes != video.SizeBytes {
		userError(c, errGenericInvalidContentRange)
		return
	}

	video.Status = models.UPLOADING
	defer video.Save()

	stream, err := files.NewResolver().AddVar(files.VideoId, video.Id).Resolve(files.VideoStream)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	file, err := stream.Open()
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}
	defer stream.AutoClose()

	_, err = file.Seek(contentRange.startByte, 0)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		stream.Remove()
		return
	}

	_, err = io.Copy(file, c.Request.Body)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		stream.Remove()
		return
	}

	file.Close()

	if (contentRange.endByte + 1) != contentRange.contentSizeBytes {
		c.Status(http.StatusAccepted)
		return
	}

	probe, err := codec.Probe(video.Id)

	if err != nil {
		stream.Remove()

		if errors.Is(err, codec.ErrInvalidVideoType) {
			userError(c, errVideoInvalidFormat)
		} else {
			serverError(c, err, errVideoProbe)
		}
		return
	}

	video.MimeType = probe.MimeType
	video.DurationSecs = probe.DurationSecs
	video.Status = models.PROCESSING

	err = codec.GenerateThumbnail(video.Id, probe)

	if ok := checkServerError(c, err, errVideoGenerateThumbnail); !ok {
		stream.Remove()
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

func GetUserVideos(c *gin.Context) {
	user := getUrlUserParam(c)

	videos, err := models.FetchVideosFor(user)
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	var visible func(*models.Video) bool
	if hasUserParam(c) && getUserParam(c).Id == user.Id {
		visible = func(_ *models.Video) bool {
			return true
		}
	} else {
		visible = func(video *models.Video) bool {
			return video.Visibility >= models.PUBLIC
		}
	}

	videoInfos := make([]*VideoInfo, 0)
	for _, video := range videos {
		if !visible(&video) {
			continue
		}

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
	video := getVideoParam(c)

	thumbnail, err := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		Resolve(files.VideoThumbnail)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	c.File(thumbnail.Path())
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

	thumbnailFile, err := files.NewResolver().
		AddVar(files.VideoId, video.Id).
		Resolve(files.VideoThumbnail)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	thumbnail, err := loadImage(jpeg.Decode, thumbnailFile.Path())

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

	c.Data(http.StatusOK, mimetype.ImageJpeg, buf.Bytes())
}

func GetVideoStream(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	stream, err := files.NewResolver().AddVar(files.VideoId, video.Id).Resolve(files.VideoStream)
	if ok := checkServerError(c, err, errGenericFileIo); !ok {
		return
	}

	c.File(stream.Path())

	bytesStreamed := int64(c.Writer.Size())
	if bytesStreamed <= 0 {
		return
	}
	err = video.TrackBytesStreamed(user, bytesStreamed)
	recordError(c, err)
}

func PostWatchHint(c *gin.Context) {
	user := getUserParam(c)
	video := getVideoParam(c)

	result, err := video.TryCountView(user)
	switch result.Code {
	case models.TryAddResultError:
		serverError(c, err, errVideoViewProcessWatchHint)
	case models.TryAddResultSuccess:
		c.Status(http.StatusNoContent)
	case models.TryAddResultDuplicateView:
		c.Status(http.StatusAlreadyReported)
	case models.TryAddResultConditionsNotSatisfied:
		c.JSON(http.StatusOK, result.Conditions)
	}
}
