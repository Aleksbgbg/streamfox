package models

import (
	"fmt"
	"streamfox-backend/files"
	"streamfox-backend/utils"

	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

var db *gorm.DB

func genUser(user User) error {
	exists, err := idExists(user.Id)
	if err != nil {
		return err
	}
	if exists {
		return nil
	}

	return user.Save()
}

func Setup() error {
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
		return fmt.Errorf("could not connect to the database: %w", err)
	}

	db.AutoMigrate(&Subtitle{})
	db.AutoMigrate(&User{})
	db.AutoMigrate(&Video{})
	db.AutoMigrate(&View{})
	db.AutoMigrate(&Watch{})

	err = setupIdGen()
	if err != nil {
		return fmt.Errorf("could not setup ID generator: %w", err)
	}

	passwordFile, err := files.NewResolver().Resolve(files.StreamfoxDefaultPassword)
	if err != nil {
		return err
	}

	streamfoxPassword, err := passwordFile.ReadOrFillIfEmpty(utils.SecureString)
	if err != nil {
		return err
	}

	streamfox := "Streamfox"
	genUser(User{
		Base:     Base{Id: IdFromInt(1)},
		Username: &streamfox,
		Password: &streamfoxPassword,
	})

	anonymous := "Anonymous"
	genUser(User{
		Base:     Base{Id: IdFromInt(2)},
		Username: &anonymous,
	})

	return nil
}
