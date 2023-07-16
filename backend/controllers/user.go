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
	if user.IsAnonymous() {
		return UserInfo{
			Id:       user.IdSnowflake().Base58(),
			Username: "Anonymous",
		}
	}

	return UserInfo{
		Id:       user.IdSnowflake().Base58(),
		Username: *user.Username,
	}
}

func GetUser(c *gin.Context) {
	user := getUserParam(c)
	c.JSON(http.StatusOK, getUserInfo(user))
}
