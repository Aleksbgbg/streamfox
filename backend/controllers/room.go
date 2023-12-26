package controllers

import (
	"streamfox-backend/models"
	"time"
)

const roomTimeout = 5 * time.Minute

type roomVisibility int8

const (
	unlistedRoom roomVisibility = iota
	publicRoom
)

type Room struct {
	id         models.Id
	name       string
	creator    models.User
	createdAt  time.Time
	visibility roomVisibility

	closed chan struct{}
}

func NewLiveRoom(name string, creator models.User, visibility roomVisibility) *Room {
	return &Room{
		id:         models.NewId(),
		creator:    creator,
		createdAt:  time.Now(),
		name:       name,
		visibility: visibility,
		closed:     make(chan struct{}),
	}
}

func (room *Room) Id() models.Id {
	return room.id
}

func (room *Room) Name() string {
	return room.name
}

func (room *Room) Creator() *models.User {
	return &room.creator
}

func (room *Room) CreatedAt() time.Time {
	return room.createdAt
}

func (room *Room) Visible() bool {
	return room.visibility >= publicRoom
}

func (room *Room) Participants() int {
	return 0
}

func (room *Room) Closed() <-chan struct{} {
	return room.closed
}
