package controllers

import (
	"net/http"
	"streamfox-backend/models"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

func authenticate(c *gin.Context, token string) {
	c.SetCookie("Authorization", token, 0, "", "", false, false)
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
