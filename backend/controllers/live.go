package controllers

import (
	"cmp"
	"net/http"
	"slices"
	"streamfox-backend/models"
	"time"

	cmap "github.com/orcaman/concurrent-map/v2"

	"github.com/gin-gonic/gin"
)

var rooms = cmap.NewStringer[models.Id, *Room]()

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
	room := NewLiveRoom(info.Name, *user, *info.Visibility)
	rooms.Set(room.Id(), room)

	go func() {
		<-room.Closed()
		rooms.Remove(room.Id())
	}()

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

func GetStreamKey(c *gin.Context) {
	user := getUserParam(c)

	token, err := generateStreamToken(user.Id)
	if ok := checkServerError(c, err, errAuthGeneratingToken); !ok {
		return
	}

	c.String(http.StatusOK, token)
}
