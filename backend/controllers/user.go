package controllers

import (
	"net/http"
	"streamfox-backend/models"

	"github.com/gin-gonic/gin"
)

type UserInfo struct {
	Id       string `json:"id"`
	Username string `json:"username"`
}

func getUserInfo(user *models.User) UserInfo {
	return UserInfo{
		Id:       user.Id.String(),
		Username: user.Name(),
	}
}

func GetUser(c *gin.Context) {
	user := getUserParam(c)
	c.JSON(http.StatusOK, getUserInfo(user))
}
