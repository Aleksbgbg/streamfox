package models

import (
	"fmt"
	"strings"

	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

type User struct {
	Base

	Username          *string `gorm:"type:varchar(32); unique;"`
	CanonicalUsername *string `gorm:"type:varchar(32); unique;"`

	EmailAddress          *string `gorm:"type:text; unique;"`
	CanonicalEmailAddress *string `gorm:"type:text; unique;"`

	Password *string `gorm:"type:char(60);"`
}

func ValidateCredentials(username, password string) (*User, error) {
	user := &User{}

	err := db.First(user, User{Username: &username}).Error

	if err != nil {
		return nil, err
	}

	err = verifyPassword(password, *user.Password)

	if err == bcrypt.ErrMismatchedHashAndPassword {
		return nil, err
	}

	return user, nil
}

func verifyPassword(password, hashedPassword string) error {
	return bcrypt.CompareHashAndPassword([]byte(hashedPassword), []byte(password))
}

func UsernameExists(username string) (bool, error) {
	lowerUsername := strings.ToLower(username)

	var count int64
	err := db.Model(&User{}).
		Where(&User{CanonicalUsername: &lowerUsername}).
		Count(&count).Error
	return count > 0, err
}

func EmailExists(email string) (bool, error) {
	lowerEmail := strings.ToLower(email)

	var count int64
	err := db.Model(&User{}).
		Where(&User{CanonicalEmailAddress: &lowerEmail}).
		Count(&count).Error
	return count > 0, err
}

func FetchUser(id Id) (*User, error) {
	user := User{}
	err := db.First(&user, id).Error
	return &user, err
}

func GenerateAnonymousUser() (*User, error) {
	user := &User{}
	err := user.Save()
	return user, err
}

func (user *User) Save() error {
	user.Id = NewId()
	if user.Username != nil {
		lowerUsername := strings.ToLower(*user.Username)
		user.CanonicalUsername = &lowerUsername
	}
	if user.EmailAddress != nil {
		lowerEmail := strings.ToLower(*user.EmailAddress)
		user.CanonicalEmailAddress = &lowerEmail
	}

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

	err = db.Model(&View{}).
		Where(View{UserId: anonymous.Id}).
		Update("user_id", user.Id).
		Error

	if err != nil {
		return err
	}

	anonWatch, err := watchOrNil(anonymous)

	if err != nil {
		return err
	}

	if anonWatch != nil {
		userWatch, err := watchOrNil(user)

		if err != nil {
			return err
		}

		if userWatch == nil {
			userWatch = &Watch{}
		}

		userWatch.UserId = user.Id
		userWatch.VideoId = anonWatch.VideoId
		userWatch.StartedAt = anonWatch.StartedAt
		userWatch.BytesStreamed = anonWatch.BytesStreamed
		err = userWatch.save()

		if err != nil {
			return err
		}

		err = db.Delete(anonWatch).Error

		if err != nil {
			return err
		}
	}

	return db.Delete(anonymous).Error
}
