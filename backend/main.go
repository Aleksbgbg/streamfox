package main

import (
	"streamfox-backend/controllers"
	"streamfox-backend/middleware"
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
	api.Use(middleware.JwtAuthMiddleware())

	api.GET("/user", controllers.GetUser)

	api.POST("/videos", controllers.CreateVideo)
	api.PUT("/videos/:id/settings", controllers.UpdateVideo)
	api.PUT("/videos/:id/content", controllers.UploadVideo)

	router.Run(":5000")
}
