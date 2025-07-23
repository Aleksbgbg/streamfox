package config

import (
	"fmt"
	"os"

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
	var configPath string
	if len(os.Args) == 1 {
		configPath = ".env"
	} else {
		configPath = os.Args[1]
	}

	if err := godotenv.Load(configPath); err != nil {
		return fmt.Errorf("error loading .env file: %w", err)
	}

	return envconfig.Process("", &Values)
}
