package models

import (
	"streamfox-backend/utils"

	"github.com/bwmarrin/snowflake"
	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

func VerifyPassword(password, hashedPassword string) error {
	return bcrypt.CompareHashAndPassword([]byte(hashedPassword), []byte(password))
}

func ValidateCredentials(username, password string) (string, error) {
	user := User{}

	err := DATABASE.Model(User{}).Where(&User{Username: username}).Take(&user).Error

	if err != nil {
		return "", err
	}

	err = VerifyPassword(password, user.Password)

	if err == bcrypt.ErrMismatchedHashAndPassword {
		return "", err
	}

	token, err := utils.GenerateToken(snowflake.ParseInt64(user.Id))

	if err != nil {
		return "", err
	}

	return token, nil
}

func FetchUser(id snowflake.ID) (User, error) {
	user := User{}
	err := DATABASE.Model(User{}).Where(&User{Base: Base{Id: id.Int64()}}).Take(&user).Error
	return user, err
}

func UsernameExists(username string) bool {
	var exists bool
	DATABASE.Model(User{}).
		Select("count(*) > 0").
		Where(&User{Username: username}).
		Find(&exists)
	return exists
}

func EmailExists(email string) bool {
	var exists bool
	DATABASE.Model(User{}).
		Select("count(*) > 0").
		Where(&User{EmailAddress: email}).
		Find(&exists)
	return exists
}

type User struct {
	Base
	Username     string `gorm:"type:varchar(32); not null; unique;"`
	EmailAddress string `gorm:"type:text; not null; unique;"`
	Password     string `gorm:"type:char(60); not null;"`
}

func (user *User) IdSnowflake() snowflake.ID {
	return snowflake.ParseInt64(user.Id)
}

func (user *User) Save() (*User, error) {
	user.Id = ID_GENERATOR.Generate().Int64()
	err := DATABASE.Create(&user).Error
	return user, err
}

func (user *User) BeforeSave(tx *gorm.DB) error {
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(user.Password), bcrypt.DefaultCost)

	if err != nil {
		return err
	}

	user.Password = string(hashedPassword)

	return nil
}
