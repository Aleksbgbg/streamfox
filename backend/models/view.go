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
	count, err := count(&View{Base: Base{Id: viewId}})
	return count > 0, err
}

func CountViews(video *Video) (int64, error) {
	count, err := count(&View{VideoId: video.Id})
	return count, err
}
