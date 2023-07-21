package main

import (
	"streamfox-backend/controllers"
	"streamfox-backend/models"

	"github.com/gin-gonic/gin"
)

func main() {
	models.Setup()

	router := gin.Default()

	api := router.Group("/api")
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
		"/stream",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.GetVideoStream,
	)
	specificVideo.GET(
		"/required-watch-time-ms",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.GetRequiredWatchTimeMs,
	)
	specificVideo.POST(
		"/still-watching",
		controllers.EnsureCompleteVideoMiddleware,
		controllers.EnsureVisibleVideoMiddleware,
		controllers.GenerateAnonymousUserMiddleware,
		controllers.StillWatching,
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

	router.Run(":5000")
}
