package models

import (
	"errors"
	"fmt"

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
	Status             VideoStatus
	MimeType           string `gorm:"type:text"`
	DurationSecs       int32
	SizeBytes          int64 `gorm:"not null"`
	SubtitlesExtracted bool  `gorm:"not null; default:false"`
}

type Settings struct {
	CreatorId   Id
	Creator     User
	Name        string `gorm:"type:text"`
	Description string `gorm:"type:text"`
	Visibility  Visibility
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

func (video *Video) IsCreator(user *User) bool {
	return video.CreatorId == user.Id
}

func (video *Video) ProcessStream(user *User, bytesStreamed int64) error {
	watch, err := watchFor(user, video)

	if err != nil {
		return fmt.Errorf("could not get watch: %v", err)
	}

	counted, err := viewAlreadyCounted(watch.ViewId)

	if err != nil {
		return err
	}

	if counted {
		return nil
	}

	*watch.BytesStreamed += bytesStreamed
	err = watch.save()

	if err != nil {
		return fmt.Errorf("could not save watch: %v", err)
	}

	return nil
}

func (video *Video) CalculateWatchConditions(user *User) (WatchConditions, error) {
	watch, err := watchFor(user, video)

	if err != nil {
		return WatchConditions{}, fmt.Errorf("could not get watch: %v", err)
	}

	return calculateWatchConditions(watch, video), nil
}

type TryAddResultCode int

const (
	ADD_VIEW_FAILURE TryAddResultCode = iota
	ADD_VIEW_SUCCESS
	ADD_VIEW_DUPLICATE
	ADD_VIEW_TIME_NOT_PASSED
	ADD_VIEW_VIDEO_NOT_STREAMED_ENOUGH
)

type TryAddResult struct {
	Code TryAddResultCode

	Conditions WatchConditions
}

func (video *Video) TryAddView(user *User) (TryAddResult, error) {
	watch, err := watchFor(user, video)

	if err != nil {
		return TryAddResult{Code: ADD_VIEW_FAILURE}, fmt.Errorf("could not get watch: %v", err)
	}

	counted, err := viewAlreadyCounted(watch.ViewId)

	if err != nil {
		return TryAddResult{
				Code: ADD_VIEW_FAILURE,
			}, fmt.Errorf(
				"could not check if view was counted: %v",
				err,
			)
	}

	if counted {
		return TryAddResult{Code: ADD_VIEW_DUPLICATE}, nil
	}

	conditions := calculateWatchConditions(watch, video)

	if conditions.RemainingBytes > 0 {
		return TryAddResult{
			Code:       ADD_VIEW_VIDEO_NOT_STREAMED_ENOUGH,
			Conditions: conditions,
		}, nil
	}

	if conditions.RemainingTime.Milliseconds() > 0 {
		return TryAddResult{
			Code:       ADD_VIEW_TIME_NOT_PASSED,
			Conditions: conditions,
		}, nil
	}

	err = recordView(View{
		Base:          Base{Id: watch.ViewId},
		UserId:        user.Id,
		VideoId:       video.Id,
		StartedAt:     watch.StartedAt,
		BytesStreamed: *watch.BytesStreamed,
	})

	if err == nil {
		return TryAddResult{Code: ADD_VIEW_SUCCESS}, nil
	}

	if errors.Is(err, gorm.ErrDuplicatedKey) {
		return TryAddResult{Code: ADD_VIEW_DUPLICATE}, nil
	}

	return TryAddResult{Code: ADD_VIEW_FAILURE}, fmt.Errorf("could not create view: %v", err)
}

func (video *Video) Save() error {
	return db.Save(video).Error
}
