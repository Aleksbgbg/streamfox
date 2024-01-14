package controllers

import (
	"bytes"
	"errors"
	"fmt"
	"html/template"
	"io/fs"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
	"streamfox-backend/config"
	"streamfox-backend/models"
	"strings"

	"github.com/abrander/ginproxy"
	"github.com/gin-gonic/gin"
	"github.com/go-http-utils/headers"
	"github.com/ldez/mimetype"
)

func ProdFrontendMiddleware(frontendPath string) gin.HandlerFunc {
	fileServer := gin.WrapH(http.FileServer(http.Dir(frontendPath)))
	return func(c *gin.Context) {
		requestedPath := filepath.Join(frontendPath, c.Request.URL.Path)
		if _, err := os.Stat(requestedPath); errors.Is(err, fs.ErrNotExist) {
			http.ServeFile(c.Writer, c.Request, filepath.Join(frontendPath, "index.html"))
		} else {
			fileServer(c)
		}
	}
}

func DevFrontendMiddleware(apiPrefix string) gin.HandlerFunc {
	g, _ := ginproxy.NewGinProxy(
		fmt.Sprintf("http://%s:%d", config.Values.DebugForwardHost, config.Values.DebugForwardPort),
	)
	return func(c *gin.Context) {
		if !strings.HasPrefix(c.Request.URL.Path, apiPrefix) {
			g.Handler(c)
		}
	}
}

var metadataInsertionMarker = []byte("<!-- metadata -->")
var watchPageRegex = regexp.MustCompile(`^/watch/(\w*)`)
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

func genDefaultMetadata(metadata *bytes.Buffer, url string, baseUrl string) {
	homePageMetadataTemplate.Execute(metadata, gin.H{
		"Description":  "Streamfox is an open-source video streaming service.",
		"Url":          url,
		"ThumbnailUrl": fmt.Sprintf("%s/thumbnail.png", baseUrl),
	})
}

func GenerateHtmlMetadata(handler gin.HandlerFunc) gin.HandlerFunc {
	return func(c *gin.Context) {
		baseBuffer := &deferredResponseWriter{
			context:  c,
			response: c.Writer,
			status:   http.StatusNotFound,
			body:     &bytes.Buffer{},
		}
		c.Writer = baseBuffer
		defer baseBuffer.Flush()

		handler(c)

		if !strings.HasPrefix(c.Writer.Header().Get(headers.ContentType), mimetype.TextHTML) {
			return
		}

		baseUrl := fmt.Sprintf("%s://%s", config.Values.AppScheme, c.Request.Host)
		url := fmt.Sprintf("%s%s", baseUrl, c.Request.URL.Path)

		var metadata bytes.Buffer

		defer func() {
			b := bytes.Replace(baseBuffer.body.Bytes(), metadataInsertionMarker, metadata.Bytes(), 1)
			baseBuffer.body = bytes.NewBuffer(b)
		}()

		match := watchPageRegex.FindStringSubmatch(c.Request.URL.Path)

		if len(match) != 2 {
			genDefaultMetadata(&metadata, url, baseUrl)
			return
		}

		videoId, err := models.IdFromString(match[1])
		if ok := recordError(c, err); !ok {
			genDefaultMetadata(&metadata, url, baseUrl)
			return
		}

		video, err := models.FetchVideo(videoId)
		if ok := recordError(c, err); !ok {
			genDefaultMetadata(&metadata, url, baseUrl)
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
	}
}
