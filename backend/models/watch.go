package models

import (
	"errors"
	"math"
	"time"

	"gorm.io/gorm"
)

type Watch struct {
	UserId Id `gorm:"primaryKey; autoIncrement:false"`
	User   User

	VideoId Id `gorm:"not null"`
	Video   Video

	ViewId Id `gorm:"not null"`

	RowMetadata

	StartedAt     time.Time `gorm:"not null"`
	BytesStreamed *int64    `gorm:"not null"`
}

const WATCH_PERCENTAGE_REQUIRED = 0.6

func watchTimeRemainingMs(watch *Watch, video *Video) int32 {
	minimumWatchTime := math.Ceil(float64(video.DurationSecs) * 1000 * WATCH_PERCENTAGE_REQUIRED)
	alreadyWatched := float64(time.Since(watch.StartedAt).Milliseconds())

	return int32(math.Max(0, math.Ceil(minimumWatchTime-alreadyWatched)))
}

func watchFor(user *User, video *Video) (*Watch, error) {
	watch := &Watch{}
	err := db.Where(Watch{UserId: user.Id}).
		Attrs(Watch{
			VideoId:       video.Id,
			ViewId:        NewId(),
			StartedAt:     time.Now(),
			BytesStreamed: new(int64),
		}).
		FirstOrCreate(watch).
		Error

	if err != nil {
		return nil, err
	}

	if watch.VideoId != video.Id {
		watch.VideoId = video.Id
		watch.ViewId = NewId()
		watch.StartedAt = time.Now()
		watch.BytesStreamed = new(int64)
		err = watch.save()

		if err != nil {
			return nil, err
		}
	}

	return watch, nil
}

func watchOrNil(user *User) (*Watch, error) {
	watch := &Watch{}
	err := db.First(watch, user.Id).Error

	if err == nil {
		return watch, nil
	}

	if errors.Is(err, gorm.ErrRecordNotFound) {
		return nil, nil
	}

	return nil, err
}

func (watch *Watch) save() error {
	return db.Save(watch).Error
}
