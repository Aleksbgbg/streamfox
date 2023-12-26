package models

import (
	"fmt"
	"streamfox-backend/config"
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
			"host=%s port=%d dbname=%s user=%s password=%s sslmode=disable TimeZone=Europe/London",
			config.Values.DbHost,
			config.Values.DbPort,
			config.Values.DbName,
			config.Values.DbUser,
			config.Values.DbPassword,
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
