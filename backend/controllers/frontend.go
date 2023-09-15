package controllers

import (
	"bytes"
	"fmt"
	"html/template"
	"net/http"
	"regexp"
	"streamfox-backend/models"
	"streamfox-backend/utils"
	"strings"

	"github.com/abrander/ginproxy"
	"github.com/gin-gonic/gin"
)

func DevFrontendMiddleware(apiPrefix string) gin.HandlerFunc {
	g, _ := ginproxy.NewGinProxy(
		fmt.Sprintf(
			"http://%s:%s",
			utils.GetEnvVar(utils.DEBUG_FORWARD_HOST),
			utils.GetEnvVar(utils.DEBUG_FORWARD_PORT),
		),
	)
	return func(c *gin.Context) {
		if !strings.HasPrefix(c.Request.URL.Path, apiPrefix) {
			g.Handler(c)
		}
	}
}

var metadataInsertionMarker = []byte("<!-- metadata -->")
var watchPageRegex = regexp.MustCompile(`/watch/(.*)$`)
var watchPageMetadataTemplate, _ = template.New("").Parse(`
<meta name="description" content="{{.Description}}">

<meta property="og:site_name" content="Streamfox">
<meta property="og:title" content="{{.Title}}">
<meta property="og:description" content="{{.Description}}">
<meta property="og:type" content="video.other">
<meta property="og:url" content="{{.Url}}">
<meta property="og:image" content="{{.ThumbnailUrl}}">
<meta property="og:video" content="{{.StreamUrl}}">
<meta property="og:video:url" content="{{.StreamUrl}}">
<meta property="og:video:secure_url" content="{{.StreamUrl}}">
<meta property="og:video:type" content="{{.MimeType}}">
`)
var homePageMetadataTemplate, _ = template.New("").Parse(`
<meta name="description" content="{{.Description}}">

<meta property="og:site_name" content="Streamfox">
<meta property="og:title" content="Streamfox">
<meta property="og:description" content="{{.Description}}">
<meta property="og:type" content="website">
<meta property="og:url" content="{{.Url}}">
<meta property="og:image" content="{{.ThumbnailUrl}}">
`)

func formatBaseUrl(c *gin.Context) string {
	return fmt.Sprintf("%s://%s", utils.GetEnvVar(utils.SCHEME), c.Request.Host)
}

func GenerateHtmlMetadataMiddleware(c *gin.Context) {
	baseBuffer := &deferredResponseWriter{
		response: c.Writer,
		status:   http.StatusNotFound,
		body:     &bytes.Buffer{},
	}
	c.Writer = baseBuffer
	defer baseBuffer.Flush()

	c.Next()

	if !strings.HasPrefix(c.Writer.Header().Get("Content-Type"), "text/html") {
		return
	}

	baseUrl := formatBaseUrl(c)
	var metadata bytes.Buffer

	url := fmt.Sprintf("%s%s", baseUrl, c.Request.URL.Path)

	match := watchPageRegex.FindStringSubmatch(c.Request.URL.Path)

	if len(match) == 2 {
		videoId, err := models.IdFromString(match[1])

		if ok := recordError(c, err); !ok {
			return
		}

		video, err := models.FetchVideo(videoId)

		if ok := recordError(c, err); !ok {
			return
		}

		watchPageMetadataTemplate.Execute(&metadata, gin.H{
			"Title":        video.Name,
			"Description":  video.Description,
			"Url":          url,
			"ThumbnailUrl": fmt.Sprintf("%s/api/videos/%s/preview", baseUrl, videoId),
			"StreamUrl":    fmt.Sprintf("%s/api/videos/%s/stream", baseUrl, videoId),
			"MimeType":     video.MimeType,
		})
	} else {
		homePageMetadataTemplate.Execute(&metadata, gin.H{
			"Description":  "Streamfox is an open-source video streaming service.",
			"Url":          url,
			"ThumbnailUrl": fmt.Sprintf("%s/thumbnail.png", baseUrl),
		})
	}

	b := bytes.Replace(baseBuffer.body.Bytes(), metadataInsertionMarker, metadata.Bytes(), 1)
	baseBuffer.body = bytes.NewBuffer(b)
}
