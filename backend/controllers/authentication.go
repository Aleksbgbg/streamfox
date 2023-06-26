package controllers

import (
	"net/http"
	"streamfox-backend/models"
	"streamfox-backend/utils"

	"github.com/gin-gonic/gin"
)

type RegisterInput struct {
	Username       string `json:"username"        binding:"required,min=2,max=32,printascii"`
	EmailAddress   string `json:"email_address"   binding:"required,email"`
	Password       string `json:"password"        binding:"required,min=6,max=72,printascii"`
	RepeatPassword string `json:"repeat_password" binding:"required,eqfield=Password"`
}

func Register(c *gin.Context) {
	var input RegisterInput

	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"errors": formatErrors(err)})
		return
	}

	if models.UsernameExists(input.Username) {
		c.JSON(
			http.StatusBadRequest,
			gin.H{"errors": gin.H{"Username": "Username must not be taken."}},
		)
		return
	}

	if models.EmailExists(input.EmailAddress) {
		c.JSON(
			http.StatusBadRequest,
			gin.H{"errors": gin.H{"EmailAddress": "Email Address must not be taken."}},
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
		c.JSON(http.StatusBadRequest, gin.H{"errors": "Error in writing to database."})
		return
	}

	token, err := utils.GenerateToken(user.IdSnowflake())

	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"errors": "Error in generating token."})
		return
	}

	c.JSON(http.StatusCreated, gin.H{"token": token})
}

type LoginInput struct {
	Username string `json:"username" binding:"required"`
	Password string `json:"password" binding:"required"`
}

func Login(c *gin.Context) {
	var input LoginInput

	if err := c.ShouldBindJSON(&input); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"errors": formatErrors(err)})
		return
	}

	token, err := models.ValidateCredentials(input.Username, input.Password)

	if err != nil {
		c.JSON(http.StatusUnauthorized, gin.H{"errors": "Invalid credentials."})
		return
	}

	c.JSON(http.StatusOK, gin.H{"token": token})
}
