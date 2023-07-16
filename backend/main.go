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
	api.Use(controllers.JwtAuthMiddleware())

	api.GET("/user", controllers.GetUser)

	api.POST("/videos", controllers.CreateVideo)
	api.GET("/videos", controllers.GetVideos)
	api.GET("/videos/:id/info", controllers.GetVideoInfo)
	api.GET("/videos/:id/thumbnail", controllers.GetVideoThumbnail)
	api.GET("/videos/:id/stream", controllers.GetVideoStream)
	api.PUT("/videos/:id/settings", controllers.UpdateVideo)
	api.PUT("/videos/:id/stream", controllers.UploadVideo)

	router.Run(":5000")
}
