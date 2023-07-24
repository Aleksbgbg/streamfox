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
	"github.com/bwmarrin/snowflake"
	"github.com/gin-gonic/gin"
)

func ProxyFrontendMiddleware(apiPrefix string) gin.HandlerFunc {
	g, _ := ginproxy.NewGinProxy(
		fmt.Sprintf(
			"http://%s:%s",
			utils.GetEnvVar(utils.DEBUG_FORWARD_HOST),
			utils.GetEnvVar(utils.DEBUG_FORWARD_PORT),
		),
	)
	return func(c *gin.Context) {
		c.Next()

		if (c.Writer.Status() == http.StatusNotFound) &&
			!strings.Contains(c.Request.URL.Path, apiPrefix) {
			g.Handler(c)
		}
	}
}

var metadataInsertionMarker = []byte("<!-- metadata -->")
var watchPageRegex = regexp.MustCompile(`/watch/(.*)$`)
var metadataTemplate, _ = template.New("watch_metadata").Parse(`
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

	if c.Writer.Header().Get("Content-Type") != "text/html" {
		return
	}

	match := watchPageRegex.FindStringSubmatch(c.Request.URL.Path)

	if len(match) != 2 {
		return
	}

	videoId := match[1]
	snowflake, err := snowflake.ParseBase58([]byte(videoId))

	if err != nil {
		c.Error(err)
		return
	}

	video, err := models.FetchVideo(snowflake)

	if err != nil {
		c.Error(err)
		return
	}

	baseUrl := formatBaseUrl(c)

	var metadata bytes.Buffer
	metadataTemplate.Execute(&metadata, gin.H{
		"Title":        video.Name,
		"Description":  video.Description,
		"Url":          fmt.Sprintf("%s%s", baseUrl, c.Request.URL.Path),
		"ThumbnailUrl": fmt.Sprintf("%s/api/videos/%s/thumbnail", baseUrl, videoId),
		"StreamUrl":    fmt.Sprintf("%s/api/videos/%s/stream", baseUrl, videoId),
		"MimeType":     video.MimeType,
	})

	b := bytes.Replace(baseBuffer.body.Bytes(), metadataInsertionMarker, metadata.Bytes(), 1)
	baseBuffer.body = bytes.NewBuffer(b)
}
