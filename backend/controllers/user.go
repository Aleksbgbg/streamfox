package controllers

import (
	"net/http"
	"streamfox-backend/models"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

type UserInfo struct {
	Id       string `json:"id"`
	Username string `json:"username"`
}

func GetUser(c *gin.Context) {
	userId, err := utils.ExtractUserId(c)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	user, err := models.FetchUser(userId)

	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	c.JSON(http.StatusOK, UserInfo{
		Id:       userId.Base58(),
		Username: user.Username,
	})
}
