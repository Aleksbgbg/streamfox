package utils

import (
	"fmt"
	"time"

	"github.com/bwmarrin/snowflake"
	"github.com/dgrijalva/jwt-go"
	"github.com/gin-gonic/gin"
)

func GenerateToken(userId snowflake.ID) (string, error) {
	tokenLifespan := GetEnvVarInt(AUTH_TOKEN_LIFESPAN_HRS)

	claims := jwt.MapClaims{}
	claims["authorized"] = true
	claims["user_id"] = userId.String()
	claims["expr"] = time.Now().Add(time.Hour * time.Duration(tokenLifespan)).Unix()

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	return token.SignedString(getApiSecret())
}

func ExtractUserId(c *gin.Context) (snowflake.ID, error) {
	token, err := parseToken(c)

	if err != nil {
		return 0, err
	}

	claims, ok := token.Claims.(jwt.MapClaims)

	if !ok || !token.Valid {
		return 0, nil
	}

	userId, err := snowflake.ParseString(claims["user_id"].(string))

	if err != nil {
		return 0, err
	}

	return userId, nil
}

func IsValidToken(c *gin.Context) error {
	_, err := parseToken(c)
	return err
}

func parseToken(c *gin.Context) (*jwt.Token, error) {
	return jwt.Parse(extractToken(c), func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}

		return getApiSecret(), nil
	})
}

func extractToken(c *gin.Context) string {
	cookie, _ := c.Cookie("Authorization")
	return cookie
}

func getApiSecret() []byte {
	return []byte(GetEnvVar(AUTH_API_SECRET))
}
