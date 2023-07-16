package controllers

import (
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

func JwtAuthMiddleware(c *gin.Context) {
	err := utils.IsValidToken(c)

	if err != nil {
		errorMessage(c, AUTHORIZATION_ERROR, "Invalid JWT.")
		c.Abort()
		return
	}

	c.Next()
}
