package controllers

import (
	"errors"
	"fmt"
	"streamfox-backend/config"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"streamfox-backend/utils"
	"time"

	"github.com/golang-jwt/jwt/v5"
)

const (
	jwtKeyUserId     = "uid"
	jwtKeyExpiration = "exp"
)

var (
	errClaimNoExist     = errors.New("claim does not exist")
	errClaimTypeInvalid = errors.New("claim type is invalid")
)

var apiSecret []byte

func SetupApiSecret() error {
	file, err := files.NewResolver().Resolve(files.AuthSecret)
	if err != nil {
		return err
	}

	secret, err := file.ReadOrFillIfEmpty(utils.SecureString)
	if err != nil {
		return err
	}

	apiSecret = []byte(secret)

	return nil
}

type token struct {
	value  string
	expiry time.Time
}

func generateToken(userId models.Id) (*token, error) {
	expiry := time.Now().Add(time.Hour * time.Duration(config.Values.AppTokenLifespanHrs))

	value, err := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		jwtKeyUserId:     userId.String(),
		jwtKeyExpiration: expiry.Unix(),
	}).SignedString(apiSecret)
	if err != nil {
		return nil, err
	}

	return &token{value, expiry}, nil
}

func getClaim[T any](claims jwt.MapClaims, key string) (T, error) {
	var result T

	value, exists := claims[key]
	if !exists {
		return result, fmt.Errorf("'%s': %+v", key, errClaimNoExist)
	}

	result, ok := value.(T)
	if !ok {
		return result, fmt.Errorf("'%s': %+v", key, errClaimTypeInvalid)
	}

	return result, nil
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

	userIdClaim, err := getClaim[string](claims, jwtKeyUserId)
	if err != nil {
		return models.NilId, err
	}

	userId, err := models.IdFromString(userIdClaim)
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
