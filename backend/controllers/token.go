package controllers

import (
	"fmt"
	"streamfox-backend/utils"
	"time"

	"github.com/bwmarrin/snowflake"
	"github.com/dgrijalva/jwt-go"
)

func generateToken(userId snowflake.ID) (string, error) {
	tokenLifespan := utils.GetEnvVarInt(utils.AUTH_TOKEN_LIFESPAN_HRS)

	claims := jwt.MapClaims{}
	claims["authorized"] = true
	claims["user_id"] = userId.String()
	claims["expr"] = time.Now().Add(time.Hour * time.Duration(tokenLifespan)).Unix()

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	return token.SignedString(getApiSecret())
}

func getUserId(tokenStr string) (snowflake.ID, error) {
	token, err := parseToken(tokenStr)

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

func parseToken(tokenStr string) (*jwt.Token, error) {
	return jwt.Parse(tokenStr, func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}

		return getApiSecret(), nil
	})
}

func getApiSecret() []byte {
	return []byte(utils.GetEnvVar(utils.AUTH_API_SECRET))
}
