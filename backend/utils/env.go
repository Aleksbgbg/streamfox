package utils

import (
	"os"
	"strconv"
)

//go:generate stringer -type=EnvKey
type EnvKey int

const (
	APP_CONFIG_ROOT EnvKey = iota
	APP_DATA_ROOT
	APP_TOKEN_LIFESPAN_HRS
	APP_SCHEME
	APP_PORT

	DB_HOST
	DB_PORT
	DB_USER
	DB_PASSWORD
	DB_NAME

	DEBUG_FORWARD_HOST
	DEBUG_FORWARD_PORT
)

func GetEnvVar(key EnvKey) string {
	return os.Getenv(key.String())
}

func GetEnvVarInt(key EnvKey) int {
	val, err := strconv.Atoi(GetEnvVar(key))

	if err != nil {
		panic(err)
	}

	return val
}
