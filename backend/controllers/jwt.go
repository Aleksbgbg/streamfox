package controllers

import (
	"net/http"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

func JwtAuthMiddleware(c *gin.Context) {
	err := utils.IsValidToken(c)

	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"errors": "Invalid Authorization JWT."})
		c.Abort()
		return
	}

	c.Next()
}
