package controllers

import (
	"fmt"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"streamfox-backend/utils"
	"time"

	"github.com/dchest/uniuri"
	"github.com/golang-jwt/jwt/v5"
)

var apiSecret []byte

func SetupApiSecret() error {
	file, err := files.NewResolver().Resolve(files.AuthSecret)
	if err != nil {
		return err
	}

	secret, err := file.ReadOrFillIfEmpty(func() string { return uniuri.New() })
	if err != nil {
		return err
	}

	apiSecret = []byte(secret)

	return nil
}

func generateToken(userId models.Id) (string, error) {
	tokenLifespan := utils.GetEnvVarInt(utils.APP_TOKEN_LIFESPAN_HRS)

	claims := jwt.MapClaims{}
	claims["authorized"] = true
	claims["user_id"] = userId.String()
	claims["expr"] = time.Now().Add(time.Hour * time.Duration(tokenLifespan)).Unix()

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	return token.SignedString(apiSecret)
}

func getUserId(tokenStr string) (models.Id, error) {
	token, err := parseToken(tokenStr)

	if err != nil {
		return models.NilId, err
	}

	claims, ok := token.Claims.(jwt.MapClaims)

	if !ok || !token.Valid {
		return models.NilId, nil
	}

	userId, err := models.IdFromString(claims["user_id"].(string))

	if err != nil {
		return models.NilId, err
	}

	return userId, nil
}

func parseToken(tokenStr string) (*jwt.Token, error) {
	return jwt.Parse(tokenStr, func(token *jwt.Token) (interface{}, error) {
		if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
			return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
		}

		return apiSecret, nil
	})
}
