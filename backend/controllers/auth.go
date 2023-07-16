package controllers

import (
	"net/http"
	"streamfox-backend/models"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

const AUTHORIZATION_COOKIE = "Authorization"

func authenticate(c *gin.Context, token string) {
	c.SetCookie(AUTHORIZATION_COOKIE, token, 0, "", "", false, false)
}

const USER_PARAM_KEY = "user"

func ExtractUserMiddleware(c *gin.Context) {
	tokenStr, err := c.Cookie(AUTHORIZATION_COOKIE)

	if err != nil {
		return
	}

	userId, err := utils.GetUserId(tokenStr)

	if err != nil {
		return
	}

	user, err := models.FetchUser(userId)

	if err != nil {
		return
	}

	c.Set(USER_PARAM_KEY, user)
}

func RequireUserMiddleware(c *gin.Context) {
	if !hasUserParam(c) {
		errorPredefined(c, USER_REQUIRED)
		c.Abort()
	}
}

func hasUserParam(c *gin.Context) bool {
	_, exists := c.Get(USER_PARAM_KEY)
	return exists
}

func getUserParam(c *gin.Context) *models.User {
	return c.MustGet(USER_PARAM_KEY).(*models.User)
}

type RegisterInput struct {
	Username       string `json:"username"       binding:"required,min=2,max=32,printascii"`
	EmailAddress   string `json:"emailAddress"   binding:"required,email"`
	Password       string `json:"password"       binding:"required,min=6,max=72,printascii"`
	RepeatPassword string `json:"repeatPassword" binding:"required,eqfield=Password"`
}

func Register(c *gin.Context) {
	var input RegisterInput

	if err := c.ShouldBindJSON(&input); err != nil {
		errorMessage(c, VALIDATION_ERROR, formatErrors(err))
		return
	}

	if models.UsernameExists(input.Username) {
		errorMessage(c, VALIDATION_ERROR, gin.H{"username": [...]string{"Username must not be taken."}})
		return
	}

	if models.EmailExists(input.EmailAddress) {
		errorMessage(
			c,
			VALIDATION_ERROR,
			gin.H{"emailAddress": [...]string{"Email Address must not be taken."}},
		)
		return
	}

	user := models.User{
		Username:     input.Username,
		EmailAddress: input.EmailAddress,
		Password:     input.Password,
	}
	_, err := user.Save()

	if err != nil {
		errorPredefined(c, DATABASE_WRITE_FAILED)
		return
	}

	token, err := utils.GenerateToken(user.IdSnowflake())

	if err != nil {
		errorMessage(c, SERVER_ERROR, "Error in generating token.")
		return
	}

	authenticate(c, token)
	c.Status(http.StatusCreated)
}

type LoginInput struct {
	Username string `json:"username" binding:"required"`
	Password string `json:"password" binding:"required"`
}

func Login(c *gin.Context) {
	var input LoginInput

	if err := c.ShouldBindJSON(&input); err != nil {
		errorMessage(c, VALIDATION_ERROR, formatErrors(err))
		return
	}

	token, err := models.ValidateCredentials(input.Username, input.Password)

	if err != nil {
		errorMessage(c, AUTHORIZATION_ERROR, "Invalid credentials.")
		return
	}

	authenticate(c, token)
	c.Status(http.StatusNoContent)
}
