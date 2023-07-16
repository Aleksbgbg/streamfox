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
	api.Use(controllers.JwtAuthMiddleware)

	api.GET("/user", controllers.GetUser)

	api.POST("/videos", controllers.CreateVideo)
	api.GET("/videos", controllers.GetVideos)

	specificVideo := api.Group("/videos/:id")
	specificVideo.Use(controllers.ExtractVideoMiddleware)
	specificVideo.GET("/info", controllers.GetVideoInfo)
	specificVideo.GET("/thumbnail", controllers.GetVideoThumbnail)
	specificVideo.GET("/stream", controllers.GetVideoStream)
	specificVideo.PUT("/settings", controllers.UpdateVideo)
	specificVideo.PUT("/stream", controllers.UploadVideo)

	router.Run(":5000")
}
