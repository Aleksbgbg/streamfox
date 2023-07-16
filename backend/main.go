package main

import (
	"streamfox-backend/controllers"
	"streamfox-backend/models"

	"github.com/gin-gonic/gin"
)

func main() {
	models.Setup()

	router := gin.Default()

	auth := router.Group("/api/auth")
	auth.POST("/register", controllers.Register)
	auth.POST("/login", controllers.Login)

	api := router.Group("/api")
	api.Use(controllers.ExtractUserMiddleware)

	api.GET("/user", controllers.RequireUserMiddleware, controllers.GetUser)

	api.POST("/videos", controllers.RequireUserMiddleware, controllers.CreateVideo)
	api.GET("/videos", controllers.GetVideos)

	specificVideo := api.Group("/videos/:id")
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
		controllers.GetVideoStream,
	)
	specificVideo.PUT("/settings", controllers.RequireUserMiddleware, controllers.UpdateVideo)
	specificVideo.PUT("/stream", controllers.RequireUserMiddleware, controllers.UploadVideo)

	router.Run(":5000")
}
