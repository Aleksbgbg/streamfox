package config

import (
	"fmt"

	"github.com/joho/godotenv"
	"github.com/kelseyhightower/envconfig"
)

type Config struct {
	AppConfigRoot       string `split_words:"true" required:"true"`
	AppDataRoot         string `split_words:"true" required:"true"`
	AppTokenLifespanHrs int    `split_words:"true" required:"true"`
	AppScheme           string `split_words:"true" required:"true"`
	AppPort             int    `split_words:"true" required:"true"`

	DbHost     string `split_words:"true" required:"true"`
	DbPort     int    `split_words:"true" required:"true"`
	DbName     string `split_words:"true" required:"true"`
	DbUser     string `split_words:"true" required:"true"`
	DbPassword string `split_words:"true" required:"true"`

	DebugForwardHost string `split_words:"true"`
	DebugForwardPort int    `split_words:"true"`
}

var Values Config

func SetupFromEnvironment() error {
	if err := godotenv.Load(".env"); err != nil {
		return fmt.Errorf("error loading .env file: %w", err)
	}

	return envconfig.Process("", &Values)
}
