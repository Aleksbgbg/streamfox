package controllers

import (
	"net/http"
	"streamfox-backend/models"

	"github.com/gin-gonic/gin"
)

const urlUserParamKey = "url_user"

func ExtractUrlUserMiddleware(c *gin.Context) {
	id := c.Param("user-id")

	userId, err := models.IdFromString(id)
	if ok := checkUserError(c, err, errUserInvalidId); !ok {
		return
	}

	user, err := models.FetchUser(userId)
	if ok := checkUserError(c, err, errUserIdNonExistent); !ok {
		return
	}

	c.Set(urlUserParamKey, user)
}

func getUrlUserParam(c *gin.Context) *models.User {
	return c.MustGet(urlUserParamKey).(*models.User)
}

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

func GetUserById(c *gin.Context) {
	user := getUrlUserParam(c)
	c.JSON(http.StatusOK, getUserInfo(user))
}
