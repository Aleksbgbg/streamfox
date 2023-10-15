package main

import (
	"fmt"
	"log"
	"streamfox-backend/controllers"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
)

func main() {
	if err := godotenv.Load(".env"); err != nil {
		log.Panicf("Error loading .env file: %v", err)
	}

	files.Setup()

	if err := controllers.SetupApiSecret(); err != nil {
		log.Panicf("Error setting up API secret: %v", err)
	}

	models.Setup()

	const API_PREFIX = "/api"
	const FRONTEND_PATH = "frontend"

	router := gin.Default()

	if gin.Mode() == gin.DebugMode {
		router.NoRoute(controllers.GenerateHtmlMetadata(controllers.DevFrontendMiddleware(API_PREFIX)))
	} else {
		router.NoRoute(controllers.GenerateHtmlMetadata(controllers.ProdFrontendMiddleware(FRONTEND_PATH)))
	}

	api := router.Group(API_PREFIX)
	api.Use(controllers.ExtractUserMiddleware)

	auth := api.Group("/auth")
	auth.POST("/register", controllers.Register)
	auth.POST("/login", controllers.Login)

	api.GET(
		"/user",
		controllers.RequireUserMiddleware,
		controllers.EnsureNotAnonymousMiddleware,
		controllers.GetUser,
	)

	videos := api.Group("/videos")
	videos.POST("", controllers.GenerateAnonymousUserMiddleware, controllers.CreateVideo)
	videos.GET("", controllers.GetVideos)
	specificVideo := videos.Group("/:id")
	specificVideo.Use(controllers.ExtractVideoMiddleware)
	specificVideo.GET(
		"/info",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GetVideoInfo,
	)
	specificVideo.GET(
		"/thumbnail",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GetVideoThumbnail,
	)
	specificVideo.GET(
		"/preview",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GetVideoPreview,
	)
	specificVideo.GET(
		"/stream",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.GetVideoStream,
	)
	specificVideo.GET(
		"/watch-conditions",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.GetWatchConditions,
	)
	specificVideo.POST(
		"/views",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.PostView,
	)
	specificVideo.PUT(
		"/settings",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.UpdateVideo,
	)
	specificVideo.PUT(
		"/stream",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.UploadVideo,
	)

	subtitles := specificVideo.Group("/subtitles")
	subtitles.Use(controllers.EnsureCompleteVideoMiddleware, controllers.EnsureVisibleVideoMiddleware)
	subtitles.GET("", controllers.GetAllSubtitles)
	subtitles.POST(
		"",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.CreateSubtitle,
	)
	subtitles.GET(
		"/info",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.GetSubtitlesInfo,
	)
	subtitles.POST(
		"/extract",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.ExtractSubtitles,
	)
	specificSubtitle := subtitles.Group("/:subtitle-id")
	specificSubtitle.Use(controllers.ExtractSubtitleMiddleware)
	specificSubtitle.PUT(
		"",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.UpdateSubtitle,
	)
	specificSubtitle.DELETE(
		"",
		controllers.RequireUserMiddleware,
		controllers.EnsureIsOwnerMiddleware,
		controllers.DeleteSubtitle,
	)
	specificSubtitle.GET("/content", controllers.GetSubtitleContent)

	router.Run(fmt.Sprintf(":%s", utils.GetEnvVar(utils.APP_PORT)))
}
