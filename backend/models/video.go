package models

import (
	"errors"

	"gorm.io/gorm"
)

type VideoStatus int8

const (
	CREATED VideoStatus = iota
	UPLOADING
	PROCESSING
	COMPLETE
)

type Visibility int8

const (
	PRIVATE Visibility = iota
	UNLISTED
	PUBLIC
)

type Metadata struct {
	Status             VideoStatus `gorm:"not null"`
	MimeType           string      `gorm:"not null; type:text"`
	DurationSecs       int32       `gorm:"not null"`
	SizeBytes          int64       `gorm:"not null"`
	SubtitlesExtracted bool        `gorm:"not null; default:false"`
}

type Settings struct {
	CreatorId   Id `gorm:"not null"`
	Creator     User
	Name        string     `gorm:"not null; type:text"`
	Description string     `gorm:"not null; type:text"`
	Visibility  Visibility `gorm:"not null"`
}

type Video struct {
	Base
	Metadata
	Settings
}

func NewVideo(creator *User) (*Video, error) {
	video := Video{
		Base: Base{
			Id: NewId(),
		},
		Metadata: Metadata{
			Status: CREATED,
		},
		Settings: Settings{
			CreatorId:  creator.Id,
			Name:       "Untitled Video",
			Visibility: PUBLIC,
		},
	}

	err := db.Create(&video).Error

	return &video, err
}

func FetchVideo(id Id) (*Video, error) {
	video := Video{}
	err := db.Preload("Creator").First(&video, id).Error
	return &video, err
}

func FetchAllVideos() ([]Video, error) {
	var videos []Video
	err := db.Preload("Creator").
		Order("id DESC").
		Find(&videos, &Video{Metadata: Metadata{Status: COMPLETE}, Settings: Settings{Visibility: PUBLIC}}).
		Error
	return videos, err
}

func FetchVideosFor(user *User) ([]Video, error) {
	var videos []Video
	err := db.Preload("Creator").
		Order("id DESC").
		Find(&videos, &Video{Settings: Settings{CreatorId: user.Id}}).
		Error
	return videos, err
}

func (video *Video) IsCreator(user *User) bool {
	return video.CreatorId == user.Id
}

func (video *Video) TrackBytesStreamed(user *User, bytesStreamed int64) error {
	watch, err := watchFor(user, video)
	if err != nil {
		return err
	}

	alreadyViewed, err := viewAlreadyCounted(watch.ViewId)
	if err != nil {
		return err
	}
	if alreadyViewed {
		return nil
	}

	*watch.BytesStreamed += bytesStreamed
	err = watch.save()
	if err != nil {
		return err
	}

	return nil
}

type TryAddResultCode int

const (
	TryAddResultError TryAddResultCode = iota
	TryAddResultSuccess
	TryAddResultDuplicateView
	TryAddResultConditionsNotSatisfied
)

type TryAddResult struct {
	Code       TryAddResultCode
	Conditions WatchConditions
}

func (video *Video) TryCountView(user *User) (TryAddResult, error) {
	var result TryAddResult

	watch, err := watchFor(user, video)
	if err != nil {
		result.Code = TryAddResultError
		return result, err
	}

	alreadyViewed, err := viewAlreadyCounted(watch.ViewId)
	if err != nil {
		result.Code = TryAddResultError
		return result, err
	}
	if alreadyViewed {
		result.Code = TryAddResultDuplicateView
		return result, nil
	}

	conditions := calculateWatchConditions(watch, video)
	if conditions.RemainingBytes > 0 {
		result.Code = TryAddResultConditionsNotSatisfied
		result.Conditions = conditions
		return result, nil
	}
	if conditions.RemainingTime.Milliseconds() > 0 {
		result.Code = TryAddResultConditionsNotSatisfied
		result.Conditions = conditions
		return result, nil
	}

	err = recordView(View{
		Base:          Base{Id: watch.ViewId},
		UserId:        user.Id,
		VideoId:       video.Id,
		StartedAt:     watch.StartedAt,
		BytesStreamed: *watch.BytesStreamed,
	})
	if err != nil {
		if errors.Is(err, gorm.ErrDuplicatedKey) {
			result.Code = TryAddResultDuplicateView
			return result, nil
		} else {
			result.Code = TryAddResultError
			return result, err
		}
	}
	result.Code = TryAddResultSuccess
	return result, nil
}

func (video *Video) Save() error {
	return db.Save(video).Error
}
