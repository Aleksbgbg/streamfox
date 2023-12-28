package controllers

import (
	"fmt"
	"streamfox-backend/e"
	"streamfox-backend/live"
	"streamfox-backend/models"
	"time"

	"github.com/pion/webrtc/v4"
)

const roomTimeout = 5 * time.Minute

type participant struct {
	user          models.User
	session       *live.RoomSession
	uploadSession *live.UploadSession
	sink          *e.Sink
}

func newParticipant(
	user *models.User,
	session *live.RoomSession,
	uploadSession *live.UploadSession,
	sink *e.Sink,
) *participant {
	return &participant{
		user:          *user,
		session:       session,
		uploadSession: uploadSession,
		sink:          sink,
	}
}

type roomVisibility int8

const (
	unlistedRoom roomVisibility = iota
	publicRoom
)

type roomEventType int

const (
	roomEventCreate roomEventType = iota
	roomEventConnect
	roomEventDisconnect
	roomEventStartStream
	roomEventStopStream
	roomEventError
)

type roomEvent struct {
	eventType   roomEventType
	participant *participant
}

func newRoomEvent(eventType roomEventType, participant *participant) roomEvent {
	return roomEvent{eventType, participant}
}

type Room struct {
	id         models.Id
	name       string
	creator    models.User
	createdAt  time.Time
	visibility roomVisibility

	exit func(*Room)

	participants map[models.Id]*participant

	events chan roomEvent
}

func NewLiveRoom(
	name string,
	creator models.User,
	visibility roomVisibility,
	exit func(*Room),
) *Room {
	room := &Room{
		id:           models.NewUnguessableId(),
		creator:      creator,
		createdAt:    time.Now(),
		name:         name,
		visibility:   visibility,
		exit:         exit,
		participants: make(map[models.Id]*participant),
		events:       make(chan roomEvent),
	}
	go room.eventLoop()
	return room
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
	return len(room.participants)
}

func (room *Room) GetParticipant(id models.Id) (*participant, bool) {
	p, exists := room.participants[id]
	return p, exists
}

func (room *Room) eventLoop() {
	roomClose := time.After(roomTimeout)

	for {
		select {
		case event := <-room.events:
			switch event.eventType {
			case roomEventCreate:
				room.create(event.participant)
			case roomEventConnect:
				roomClose = nil
				room.connect(event.participant)
			case roomEventDisconnect:
				room.disconnect(event.participant)
				if len(room.participants) == 0 {
					roomClose = time.After(roomTimeout)
				}
			case roomEventStartStream:
				room.startStream(event.participant)
			case roomEventStopStream:
				room.tryStopStream(event.participant)
			case roomEventError:
				event.participant.sink.LogReport(
					fmt.Sprintf(
						"User %s(%s) in room %s(%s) failed:",
						event.participant.user.Name(),
						event.participant.user.Id,
						room.name,
						room.id,
					),
					e.LogLevelIgnored,
				)
				room.disconnect(event.participant)
			}
		case <-roomClose:
			room.exit(room)
			return
		}
	}
}

func (room *Room) join(
	user *models.User,
	offer webrtc.SessionDescription,
	uploadSession *live.UploadSession,
	exit func(),
) (*live.RoomSession, error) {
	sink := e.NewSink()
	session, err := live.NewRoomSession(offer, sink)
	if err != nil {
		return nil, err
	}

	p := newParticipant(user, session, uploadSession, sink)
	go func() {
		defer exit()

		select {
		case <-sink.Failed():
			room.events <- newRoomEvent(roomEventError, p)
			return
		case <-session.Connected():
			room.events <- newRoomEvent(roomEventConnect, p)
		}

		select {
		case <-sink.Failed():
			room.events <- newRoomEvent(roomEventError, p)
		case <-session.Disconnected():
			room.events <- newRoomEvent(roomEventDisconnect, p)
		}
	}()

	room.events <- newRoomEvent(roomEventCreate, p)

	return session, nil
}

func (room *Room) create(p *participant) {
	if other, exists := room.participants[p.user.Id]; exists {
		room.disconnect(other)
	}

	room.participants[p.user.Id] = p
}

func (room *Room) connect(p *participant) {
	if ok := sendOne(p, newMessageConnected(getUserInfo(&p.user))); !ok {
		return
	}

	for _, other := range room.participants {
		if p.user.Id == other.user.Id {
			continue
		}

		if ok := sendOne(p, newMessageConnected(getUserInfo(&other.user))); !ok {
			return
		}
		other.trySendStream(p)
	}

	room.sendOthers(p, newMessageConnected(getUserInfo(&p.user)))

	if p.uploadSession != nil {
		room.startStream(p)
	}
}

func (room *Room) disconnect(p *participant) {
	room.tryStopStream(p)
	room.sendOthers(p, newMessageDisconnected(getUserInfo(&p.user)))
	delete(room.participants, p.user.Id)

	p.sink.Check("could not close session", p.session.Close(), e.Silence(), e.Ignore())
}

func (room *Room) startStream(p *participant) {
	for _, other := range room.participants {
		p.trySendStream(other)
	}
}

func (p *participant) trySendStream(other *participant) {
	if p.uploadSession == nil {
		return
	}

	err := other.session.Join(p.uploadSession)
	if ok := p.sink.Check("could not join session", err, e.Silence(), e.Ignore()); !ok {
		return
	}

	sendOne(other, newMessageStartStream(p.user.Id.String()))
}

func (room *Room) tryStopStream(p *participant) {
	if p.uploadSession == nil {
		return
	}

	p.uploadSession = nil
	room.sendAll(newMessageStopStream(p.user.Id.String()))
}

func (room *Room) sendAll(m message) {
	for _, p := range room.participants {
		sendOne(p, m)
	}
}

func (room *Room) sendOthers(exclude *participant, m message) {
	for _, p := range room.participants {
		if p.user.Id == exclude.user.Id {
			continue
		}

		sendOne(p, m)
	}
}

func sendOne(p *participant, m message) bool {
	err := p.session.Send(m)
	return p.sink.Check("could not send message", err, e.Silence(), e.Ignore())
}
