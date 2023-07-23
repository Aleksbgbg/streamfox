package utils

import (
	"os"
	"strconv"
)

//go:generate stringer -type=EnvKey
type EnvKey int

const (
	DATA_ROOT EnvKey = iota

	DB_HOST
	DB_PORT
	DB_USER
	DB_PASSWORD
	DB_NAME

	AUTH_API_SECRET
	AUTH_TOKEN_LIFESPAN_HRS

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
