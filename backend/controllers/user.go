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

func getUserInfo(user *models.User) UserInfo {
	return UserInfo{
		Id:       user.IdSnowflake().Base58(),
		Username: user.Username,
	}
}

func GetUser(c *gin.Context) {
	userId, err := utils.ExtractUserId(c)

	if err != nil {
		errorPredefined(c, USER_FETCH_FAILED)
		return
	}

	user, err := models.FetchUser(userId)

	if err != nil {
		errorPredefined(c, USER_FETCH_FAILED)
		return
	}

	c.JSON(http.StatusOK, getUserInfo(&user))
}
