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
	jwtKeyUsage      = "use"
)

var (
	errClaimNoExist     = errors.New("claim does not exist")
	errClaimTypeInvalid = errors.New("claim type is invalid")
	errWrongUsage       = errors.New("token usage does not match expected usage")
)

type jwtUsage int

const (
	jwtUsageLogin jwtUsage = iota
	jwtUsageStreaming
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

func generateLoginToken(userId models.Id) (*token, error) {
	expiry := time.Now().Add(time.Hour * time.Duration(config.Values.AppTokenLifespanHrs))

	value, err := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		jwtKeyUserId:     userId.String(),
		jwtKeyExpiration: expiry.Unix(),
		jwtKeyUsage:      jwtUsageLogin,
	}).SignedString(apiSecret)
	if err != nil {
		return nil, err
	}

	return &token{value, expiry}, nil
}

func generateStreamToken(userId models.Id) (string, error) {
	// Stream keys intentionally do not expire to improve the user experience (avoid having to change
	// stream keys). In the future we should add a way for the user to generate stream keys and revoke
	// them, to mitigate key theft.
	return jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{
		jwtKeyUserId: userId.String(),
		jwtKeyUsage:  jwtUsageStreaming,
	}).SignedString(apiSecret)
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

func getClaimInt(claims jwt.MapClaims, key string) (int, error) {
	claim, err := getClaim[float64](claims, key)
	if err != nil {
		return 0, err
	}

	return int(claim), nil
}

func getUserId(tokenStr string, usage jwtUsage) (models.Id, error) {
	token, err := parseToken(tokenStr)
	if err != nil {
		return models.NilId, err
	}

	claims, ok := token.Claims.(jwt.MapClaims)
	if !ok || !token.Valid {
		return models.NilId, nil
	}

	usageClaim, err := getClaimInt(claims, jwtKeyUsage)
	if err != nil {
		return models.NilId, err
	}
	if jwtUsage(usageClaim) != usage {
		return models.NilId, errWrongUsage
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
