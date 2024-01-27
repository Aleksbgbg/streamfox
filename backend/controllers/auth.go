package controllers

import (
	"net/http"
	"streamfox-backend/models"
	"time"

	"github.com/gin-gonic/gin"
)

const AUTHORIZATION_COOKIE = "Authorization"

func setAuthCookie(c *gin.Context, token string, expiry time.Time) {
	c.SetCookie(AUTHORIZATION_COOKIE, token, int(time.Until(expiry).Seconds()), "", "", false, false)
}

func authenticate(c *gin.Context, user *models.User) error {
	token, err := generateToken(user.Id)

	if err != nil {
		return err
	}

	setAuthCookie(c, token.value, token.expiry)
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

	if ok := checkServerError(c, err, errAuthGeneratingUser); !ok {
		return
	}

	err = authenticate(c, user)

	if ok := checkServerError(c, err, errAuthGeneratingToken); !ok {
		return
	}

	c.Set(USER_PARAM_KEY, user)
}

func RequireUserMiddleware(c *gin.Context) {
	if !hasUserParam(c) {
		userError(c, errUserRequired)
	}
}

func EnsureNotAnonymousMiddleware(c *gin.Context) {
	if getUserParam(c).IsAnonymous() {
		userError(c, errUserRequired)
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

	if ok := checkValidationError(c, c.ShouldBindJSON(&input)); !ok {
		return
	}

	usernameExists, err := models.UsernameExists(input.Username)
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}
	if usernameExists {
		validationError(c, errorMap{"username": []string{"Username must not be taken."}})
		return
	}

	emailExists, err := models.EmailExists(input.EmailAddress)
	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}
	if emailExists {
		validationError(c, errorMap{"emailAddress": []string{"Email Address must not be taken."}})
		return
	}

	user := &models.User{
		Username:     &input.Username,
		EmailAddress: &input.EmailAddress,
		Password:     &input.Password,
	}
	err = user.Save()

	if ok := checkServerError(c, err, errGenericDatabaseIo); !ok {
		return
	}

	err = absorbAnonymousUser(c, user)

	if ok := checkServerError(c, err, errUserMergeFailed); !ok {
		return
	}

	err = authenticate(c, user)

	if ok := checkServerError(c, err, errAuthGeneratingToken); !ok {
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

	if ok := checkValidationError(c, c.ShouldBindJSON(&input)); !ok {
		return
	}

	user, err := models.ValidateCredentials(input.Username, input.Password)

	if ok := checkUserError(c, err, errAuthInvalidCredentials); !ok {
		return
	}

	err = absorbAnonymousUser(c, user)

	if ok := checkServerError(c, err, errUserMergeFailed); !ok {
		return
	}

	err = authenticate(c, user)

	if ok := checkServerError(c, err, errAuthGeneratingToken); !ok {
		return
	}

	c.Status(http.StatusNoContent)
}
