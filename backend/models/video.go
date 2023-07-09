package models

import "github.com/bwmarrin/snowflake"

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
	Status       VideoStatus
	MimeType     string `gorm:"type:text"`
	DurationSecs int32
}

type Settings struct {
	CreatorId   int64
	Creator     User
	Name        string `gorm:"type:text"`
	Description string `gorm:"type:text"`
	Visibility  Visibility
}

type Statistics struct {
	Views    int64
	Likes    int64
	Dislikes int64
}

type Video struct {
	Base
	Metadata
	Settings
	Statistics
}

func NewVideo(creatorId snowflake.ID) (*Video, error) {
	video := Video{
		Base: Base{
			Id: ID_GENERATOR.Generate().Int64(),
		},
		Metadata: Metadata{
			Status: CREATED,
		},
		Settings: Settings{
			CreatorId:  creatorId.Int64(),
			Name:       "Untitled Video",
			Visibility: PUBLIC,
		},
	}

	err := DATABASE.Create(&video).Error

	return &video, err
}

func FetchVideo(id snowflake.ID) (*Video, error) {
	video := Video{}
	err := DATABASE.First(&video, id.Int64()).Error
	return &video, err
}

func FetchVideoWithOwner(id snowflake.ID) (*Video, error) {
	video := Video{}
	err := DATABASE.Preload("Creator").First(&video, id.Int64()).Error
	return &video, err
}

func FetchAllVideos() ([]Video, error) {
	users := make([]Video, 0)
	err := DATABASE.Preload("Creator").
		Find(&users, &Video{Metadata: Metadata{Status: COMPLETE}, Settings: Settings{Visibility: PUBLIC}}).
		Error
	return users, err
}

func (video *Video) IdSnowflake() snowflake.ID {
	return snowflake.ParseInt64(video.Id)
}

func (video *Video) IsCreator(userId snowflake.ID) bool {
	return video.CreatorId == userId.Int64()
}

func (video *Video) Save() error {
	return DATABASE.Save(video).Error
}
