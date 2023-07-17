package controllers

import (
	"net/http"
	"streamfox-backend/models"

	"github.com/gin-gonic/gin"
)

const AUTHORIZATION_COOKIE = "Authorization"

func setAuthCookie(c *gin.Context, token string) {
	c.SetCookie(AUTHORIZATION_COOKIE, token, 0, "", "", false, false)
}

func authenticate(c *gin.Context, user *models.User) error {
	token, err := generateToken(user.IdSnowflake())

	if err != nil {
		return err
	}

	setAuthCookie(c, token)
	return nil
}

const USER_PARAM_KEY = "user"

func ExtractUserMiddleware(c *gin.Context) {
	tokenStr, err := c.Cookie(AUTHORIZATION_COOKIE)

	if err != nil {
		return
	}

	userId, err := getUserId(tokenStr)

	if err != nil {
		return
	}

	user, err := models.FetchUser(userId)

	if err != nil {
		return
	}

	c.Set(USER_PARAM_KEY, user)
}

func GenerateAnonymousUserMiddleware(c *gin.Context) {
	if hasUserParam(c) {
		return
	}

	user, err := models.GenerateAnonymousUser()

	if err != nil {
		errorMessage(c, SERVER_ERROR, "Error in generating user.")
		c.Abort()
		return
	}

	err = authenticate(c, user)

	if err != nil {
		errorMessage(c, SERVER_ERROR, "Error in generating token.")
		c.Abort()
		return
	}

	c.Set(USER_PARAM_KEY, user)
}

func RequireUserMiddleware(c *gin.Context) {
	if hasUserParam(c) {
		return
	}

	errorPredefined(c, USER_REQUIRED)
	c.Abort()
}

func EnsureNotAnonymousMiddleware(c *gin.Context) {
	user := getUserParam(c)

	if user.IsAnonymous() {
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

func absorbAnonymousUser(c *gin.Context, user *models.User) error {
	if !hasUserParam(c) {
		return nil
	}

	existingUser := getUserParam(c)

	if !existingUser.IsAnonymous() {
		return nil
	}

	return user.Absorb(existingUser)
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

	user := &models.User{
		Username:     &input.Username,
		EmailAddress: &input.EmailAddress,
		Password:     &input.Password,
	}
	err := user.Save()

	if err != nil {
		errorPredefined(c, DATABASE_WRITE_FAILED)
		return
	}

	err = absorbAnonymousUser(c, user)

	if err != nil {
		errorPredefined(c, USER_MERGE_FAILED)
		return
	}

	err = authenticate(c, user)

	if err != nil {
		errorMessage(c, SERVER_ERROR, "Error in generating token.")
		c.Abort()
		return
	}

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

	user, err := models.ValidateCredentials(input.Username, input.Password)

	if err != nil {
		errorMessage(c, VALIDATION_ERROR, "Invalid credentials.")
		return
	}

	err = absorbAnonymousUser(c, user)

	if err != nil {
		errorPredefined(c, USER_MERGE_FAILED)
		return
	}

	err = authenticate(c, user)

	if err != nil {
		errorMessage(c, SERVER_ERROR, "Error in generating token.")
		c.Abort()
		return
	}

	c.Status(http.StatusNoContent)
}
