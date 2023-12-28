package controllers

import (
	"cmp"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"slices"
	"streamfox-backend/live"
	"streamfox-backend/models"
	"strings"
	"time"

	"github.com/go-http-utils/headers"
	"github.com/ldez/mimetype"
	cmap "github.com/orcaman/concurrent-map/v2"
	"github.com/pion/webrtc/v4"

	"github.com/gin-gonic/gin"
)

var rooms = cmap.NewStringer[models.Id, *Room]()
var uploadSessionsBySessionId = cmap.NewStringer[models.Id, *UploadSession]()
var uploadSessionsByUserId = cmap.NewStringer[models.Id, *live.UploadSession]()
var roomsForUser = cmap.NewStringer[models.Id, cmap.ConcurrentMap[models.Id, struct{}]]()
var roomSessionsById = cmap.NewStringer[models.Id, *RoomSession]()

type UploadSession struct {
	userId models.Id
	upload *live.UploadSession
}

type RoomSession struct {
	userId  models.Id
	session *live.RoomSession
}

const liveRoomParamKey = "live_room"

func ExtractLiveRoomMiddleware(c *gin.Context) {
	roomId, err := models.IdFromString(c.Param("id"))
	if ok := checkUserError(c, err, errLiveInvalidRoomId); !ok {
		return
	}

	room, exists := rooms.Get(roomId)
	if !exists {
		userError(c, errLiveRoomIdNonExistent)
		return
	}

	c.Set(liveRoomParamKey, room)
}

func getLiveRoomParam(c *gin.Context) *Room {
	return c.MustGet(liveRoomParamKey).(*Room)
}

type LiveRoomInfo struct {
	Id           string    `json:"id"`
	Name         string    `json:"name"`
	Creator      UserInfo  `json:"creator"`
	CreatedAt    time.Time `json:"createdAt"`
	Participants int       `json:"participants"`
}

func getLiveRoomInfo(room *Room) LiveRoomInfo {
	return LiveRoomInfo{
		Id:           room.Id().String(),
		Creator:      getUserInfo(room.Creator()),
		CreatedAt:    room.CreatedAt(),
		Name:         room.Name(),
		Participants: room.Participants(),
	}
}

func GetLiveRooms(c *gin.Context) {
	liveRoomInfos := make([]LiveRoomInfo, 0, rooms.Count())
	for pair := range rooms.IterBuffered() {
		room := pair.Val

		if !room.Visible() {
			continue
		}

		liveRoomInfos = append(liveRoomInfos, getLiveRoomInfo(room))
	}

	slices.SortFunc(liveRoomInfos, func(a LiveRoomInfo, b LiveRoomInfo) int {
		comparison := cmp.Compare(b.Participants, a.Participants)

		if comparison == 0 {
			return cmp.Compare(a.CreatedAt.Unix(), b.CreatedAt.Unix())
		} else {
			return comparison
		}
	})

	c.JSON(http.StatusOK, liveRoomInfos)
}

type CreateLiveRoomInfo struct {
	Name       string          `json:"name"       binding:"required,min=2,max=256"`
	Visibility *roomVisibility `json:"visibility" binding:"required,min=0,max=1"`
}

type LiveRoomCreatedInfo struct {
	Id string `json:"id"`
}

func CreateLiveRoom(c *gin.Context) {
	var info CreateLiveRoomInfo
	if ok := checkValidationError(c, c.ShouldBindJSON(&info)); !ok {
		return
	}

	user := getUserParam(c)
	room := NewLiveRoom(info.Name, *user, *info.Visibility, func(room *Room) {
		rooms.Remove(room.Id())
	})
	rooms.Set(room.Id(), room)

	c.JSON(http.StatusCreated, LiveRoomCreatedInfo{room.Id().String()})
}

func GetLiveRoom(c *gin.Context) {
	room := getLiveRoomParam(c)
	info := getLiveRoomInfo(room)
	c.JSON(http.StatusOK, info)
}

func GetLiveRoomThumbnail(c *gin.Context) {
	c.Status(http.StatusNotFound)
}

const sessionIdKey = "session_id"

func ExtractSessionIdMiddleware(c *gin.Context) {
	sessionIdString := c.Param("session-id")

	sessionId, err := models.IdFromString(sessionIdString)
	if ok := checkUserError(c, err, errLiveInvalidSessionId); !ok {
		return
	}

	c.Set(sessionIdKey, sessionId)
}

func getSessionIdParam(c *gin.Context) models.Id {
	return c.MustGet(sessionIdKey).(models.Id)
}

const streamingUserKey = "streaming_user"

func ExtractStreamingUserMiddleware(c *gin.Context) {
	auth := strings.Split(c.GetHeader(headers.Authorization), " ")
	if len(auth) != 2 {
		userError(c, errAuthInvalidBearerFormat)
		return
	}

	userId, err := getUserId(auth[1], jwtUsageStreaming)
	if err != nil {
		userError(c, errUserRequired)
		return
	}

	c.Set(streamingUserKey, userId)
}

func getStreamingUserParam(c *gin.Context) models.Id {
	return c.MustGet(streamingUserKey).(models.Id)
}

func GetStreamKey(c *gin.Context) {
	user := getUserParam(c)

	token, err := generateStreamToken(user.Id)
	if ok := checkServerError(c, err, errAuthGeneratingToken); !ok {
		return
	}

	c.String(http.StatusOK, token)
}

func forAllRooms(userId models.Id, f func(*Room, *participant)) {
	userRooms, exists := roomsForUser.Get(userId)
	if !exists {
		return
	}

	for pair := range userRooms.IterBuffered() {
		room, exists := rooms.Get(pair.Key)
		if !exists {
			continue
		}

		p, exists := room.GetParticipant(userId)
		if !exists {
			continue
		}

		f(room, p)
	}
}

func BeginUploadSession(c *gin.Context) {
	userId := getStreamingUserParam(c)

	_, exists := uploadSessionsByUserId.Get(userId)
	if exists {
		userError(c, errLiveAlreadyStreaming)
		return
	}

	body, err := io.ReadAll(c.Request.Body)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	user, err := models.FetchUser(userId)
	if ok := checkServerError(c, err, errUserRequired); !ok {
		return
	}

	sessionId := models.NewUnguessableId()
	uploadSession, err := live.NewUploadSession(
		webrtc.SessionDescription{Type: webrtc.SDPTypeOffer, SDP: string(body)},
		user,
		func(uploadSession *live.UploadSession) {
			uploadSessionsByUserId.Set(userId, uploadSession)
			forAllRooms(userId, func(room *Room, p *participant) {
				p.uploadSession = uploadSession
				room.events <- newRoomEvent(roomEventStartStream, p)
			})
		},
		func(uploadSession *live.UploadSession) {
			log.Println("Remove :)")
			forAllRooms(userId, func(room *Room, p *participant) {
				room.events <- newRoomEvent(roomEventStopStream, p)
			})
			log.Println("Defo :)")
			uploadSessionsByUserId.Remove(userId)
			log.Println("Certi :)")
			_, exists := uploadSessionsByUserId.Get(userId)
			log.Println("Certi2 :)", exists)
			uploadSessionsBySessionId.Remove(sessionId)
		},
	)
	if ok := checkServerError(c, err, errLiveBeginUpload); !ok {
		return
	}

	s := &UploadSession{
		userId: user.Id,
		upload: uploadSession,
	}
	uploadSessionsBySessionId.Set(sessionId, s)

	c.Header(headers.Location, fmt.Sprintf("%s/%s", c.Request.URL.Path, sessionId))
	c.Header(headers.ContentType, mimetype.ApplicationSdp)
	c.String(http.StatusCreated, uploadSession.Description().SDP)
}

func EndUploadSession(c *gin.Context) {
	userId := getStreamingUserParam(c)
	sessionId := getSessionIdParam(c)

	uploadSession, exists := uploadSessionsBySessionId.Get(sessionId)
	if !exists {
		userError(c, errLiveSessionIdNonExistent)
		return
	}

	if uploadSession.userId != userId {
		userError(c, errLiveSessionNotOwned)
		return
	}

	err := uploadSession.upload.Close()
	recordError(c, err)

	c.Status(http.StatusNoContent)
}

const roomSessionKey = "room_session"

func ExtractRoomSessionMiddleware(c *gin.Context) {
	user := getUserParam(c)
	sessionId := getSessionIdParam(c)

	s, exists := roomSessionsById.Get(sessionId)
	if !exists {
		userError(c, errLiveSessionIdNonExistent)
		return
	}

	if s.userId != user.Id {
		userError(c, errLiveSessionNotOwned)
		return
	}

	c.Set(roomSessionKey, s)
}

func getRoomSessionParam(c *gin.Context) *RoomSession {
	return c.MustGet(roomSessionKey).(*RoomSession)
}

type RoomJoinedInfo struct {
	SessionId   string                    `json:"sessionId"`
	Description webrtc.SessionDescription `json:"description"`
}

func JoinRoom(c *gin.Context) {
	body, err := io.ReadAll(c.Request.Body)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	offer := webrtc.SessionDescription{}
	err = json.Unmarshal(body, &offer)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	room := getLiveRoomParam(c)
	user := getUserParam(c)
	uploadSession, _ := uploadSessionsByUserId.Get(user.Id)
	sessionId := models.NewUnguessableId()

	roomsForUser.SetIfAbsent(user.Id, cmap.NewStringer[models.Id, struct{}]())
	roomSet, _ := roomsForUser.Get(user.Id)
	roomSet.Set(room.id, struct{}{})
	session, err := room.join(user, offer, uploadSession, func() {
		roomSessionsById.Remove(sessionId)
		roomSet.Remove(room.id)
	})
	if ok := checkServerError(c, err, errLiveJoinSession); !ok {
		return
	}

	roomSessionsById.Set(sessionId, &RoomSession{
		userId:  user.Id,
		session: session,
	})

	c.JSON(http.StatusCreated, RoomJoinedInfo{
		SessionId:   sessionId.String(),
		Description: session.Description(),
	})
}

func Renegotiate(c *gin.Context) {
	body, err := io.ReadAll(c.Request.Body)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	offer := webrtc.SessionDescription{}
	err = json.Unmarshal(body, &offer)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	session := getRoomSessionParam(c)

	desc, err := session.session.Renegotiate(offer)
	if ok := checkServerError(c, err, errLiveRenegotiateSession); !ok {
		return
	}

	c.JSON(http.StatusOK, desc)
}

func TrickeCandidate(c *gin.Context) {
	body, err := io.ReadAll(c.Request.Body)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	candidate := webrtc.ICECandidateInit{}
	err = json.Unmarshal(body, &candidate)
	if ok := checkServerError(c, err, errGenericSocketIo); !ok {
		return
	}

	session := getRoomSessionParam(c)
	err = session.session.AddIceCandidate(candidate)
	recordError(c, err)

	c.Status(http.StatusNoContent)
}
