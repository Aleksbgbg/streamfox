package models

import (
	"fmt"

	"github.com/bwmarrin/snowflake"
	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

func VerifyPassword(password, hashedPassword string) error {
	return bcrypt.CompareHashAndPassword([]byte(hashedPassword), []byte(password))
}

func ValidateCredentials(username, password string) (*User, error) {
	user := &User{}

	err := db.First(user, User{Username: &username}).Error

	if err != nil {
		return nil, err
	}

	err = VerifyPassword(password, *user.Password)

	if err == bcrypt.ErrMismatchedHashAndPassword {
		return nil, err
	}

	return user, nil
}

func FetchUser(id snowflake.ID) (*User, error) {
	user := User{}
	err := db.First(&user, id.Int64()).Error
	return &user, err
}

func UsernameExists(username string) bool {
	var exists bool
	db.Model(User{}).
		Select("count(*) > 0").
		Where(&User{Username: &username}).
		Find(&exists)
	return exists
}

func EmailExists(email string) bool {
	var exists bool
	db.Model(User{}).
		Select("count(*) > 0").
		Where(&User{EmailAddress: &email}).
		Find(&exists)
	return exists
}

func GenerateAnonymousUser() (*User, error) {
	user := &User{}
	err := user.Save()
	return user, err
}

type User struct {
	Base
	Username     *string `gorm:"type:varchar(32); unique;"`
	EmailAddress *string `gorm:"type:text; unique;"`
	Password     *string `gorm:"type:char(60);"`
}

func (user *User) IdSnowflake() snowflake.ID {
	return snowflake.ParseInt64(user.Id)
}

func (user *User) IsAnonymous() bool {
	return user.Username == nil
}

// All data relevant to the anonymous user is merged into user, after which the anonymous user is
// deleted from the database. Anonymous is no longer a valid user when this function call succeeds.
func (user *User) Absorb(anonymous *User) error {
	if !anonymous.IsAnonymous() {
		return fmt.Errorf(
			"attempting to absorb non-anonymous user %d into %d - this is incorrect",
			anonymous.Id,
			user.Id,
		)
	}

	if user.IsAnonymous() {
		return fmt.Errorf(
			"attempting to absorb anonymous user %d into another anonymous user %d - this is incorrect",
			anonymous.Id,
			user.Id,
		)
	}

	err := db.Model(&Video{}).
		Where(Video{Settings: Settings{CreatorId: anonymous.Id}}).
		Update("creator_id", user.Id).
		Error

	if err != nil {
		return err
	}

	err = db.Delete(anonymous).Error

	return err
}

func (user *User) Save() error {
	user.Id = idgen.Generate().Int64()
	err := db.Create(&user).Error
	return err
}

func (user *User) BeforeSave(tx *gorm.DB) error {
	if user.Password == nil {
		return nil
	}

	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(*user.Password), bcrypt.DefaultCost)

	if err != nil {
		return err
	}

	hashedPasswordStr := string(hashedPassword)
	user.Password = &hashedPasswordStr

	return nil
}
