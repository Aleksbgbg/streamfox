package models

import (
	"time"
)

type View struct {
	Base

	UserId Id `gorm:"not null"`
	User   User

	VideoId Id `gorm:"not null"`
	Video   Video

	StartedAt     time.Time `gorm:"not null"`
	BytesStreamed int64     `gorm:"not null"`
}

func recordView(view View) error {
	return db.Create(&view).Error
}

func viewAlreadyCounted(viewId Id) (bool, error) {
	var count int64
	err := db.Model(&View{}).Where(&View{Base: Base{Id: viewId}}).Count(&count).Error

	if err != nil {
		return false, err
	}

	return count > 0, nil
}

func CountViews(video *Video) (int64, error) {
	var count int64
	err := db.Model(&View{}).Where(&View{VideoId: video.Id}).Count(&count).Error
	return count, err
}
