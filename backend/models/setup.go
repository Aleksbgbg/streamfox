package models

import (
	"fmt"
	"log"
	"streamfox-backend/utils"

	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

var db *gorm.DB

func Setup() {
	var err error
	db, err = gorm.Open(
		postgres.Open(fmt.Sprintf(
			"host=%s user=%s password=%s dbname=%s port=%s sslmode=disable TimeZone=Europe/London",
			utils.GetEnvVar(utils.DB_HOST),
			utils.GetEnvVar(utils.DB_USER),
			utils.GetEnvVar(utils.DB_PASSWORD),
			utils.GetEnvVar(utils.DB_NAME),
			utils.GetEnvVar(utils.DB_PORT),
		)),
		&gorm.Config{
			TranslateError: true,
		},
	)

	if err != nil {
		log.Fatal("Could not connect to database:", err)
	}

	db.AutoMigrate(&Subtitle{})
	db.AutoMigrate(&User{})
	db.AutoMigrate(&Video{})
	db.AutoMigrate(&View{})
	db.AutoMigrate(&Watch{})

	err = setupIdGen()

	if err != nil {
		log.Panicf("could not setup ID generator: %v", err)
	}
}
