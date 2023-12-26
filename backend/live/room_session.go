package live

import (
	"encoding/json"
	"streamfox-backend/e"

	"github.com/pion/webrtc/v4"
)

type RoomSession struct {
	connection *webrtc.PeerConnection
	channel    *webrtc.DataChannel

	connected    chan struct{}
	disconnected chan struct{}

	sink *e.Sink
}

func NewRoomSession(offer webrtc.SessionDescription, sink *e.Sink) (*RoomSession, error) {
	api, err := newWebRtcApi()
	if err != nil {
		return nil, err
	}
	peerConnection, err := api.NewPeerConnection(configs)
	if err != nil {
		return nil, err
	}

	session := &RoomSession{
		connection:   peerConnection,
		channel:      nil,
		connected:    make(chan struct{}),
		disconnected: make(chan struct{}),
		sink:         sink,
	}

	peerConnection.OnConnectionStateChange(func(s webrtc.PeerConnectionState) {
		if s == webrtc.PeerConnectionStateDisconnected ||
			s == webrtc.PeerConnectionStateClosed ||
			s == webrtc.PeerConnectionStateFailed {
			session.disconnected <- struct{}{}
		}
	})

	peerConnection.OnDataChannel(func(channel *webrtc.DataChannel) {
		session.channel = channel

		channel.OnOpen(func() {
			session.connected <- struct{}{}
		})
		channel.OnError(func(err error) {
			sink.Fail(err)
		})
	})

	err = peerConnection.SetRemoteDescription(offer)
	if err != nil {
		return nil, err
	}

	answer, err := peerConnection.CreateAnswer(nil)
	if err != nil {
		return nil, err
	}

	err = peerConnection.SetLocalDescription(answer)
	if err != nil {
		return nil, err
	}

	<-webrtc.GatheringCompletePromise(peerConnection)

	return session, nil
}

func (session *RoomSession) Close() error {
	return session.connection.Close()
}

func (session *RoomSession) Renegotiate(
	offer webrtc.SessionDescription,
) (answer webrtc.SessionDescription, err error) {
	err = session.connection.SetRemoteDescription(offer)
	if err != nil {
		return
	}

	answer, err = session.connection.CreateAnswer(nil)
	if err != nil {
		return
	}

	err = session.connection.SetLocalDescription(answer)
	if err != nil {
		return
	}

	<-webrtc.GatheringCompletePromise(session.connection)

	return
}

func (session *RoomSession) Connected() <-chan struct{} {
	return session.connected
}

func (session *RoomSession) Disconnected() <-chan struct{} {
	return session.disconnected
}

func (session *RoomSession) AddIceCandidate(candidate webrtc.ICECandidateInit) error {
	return session.connection.AddICECandidate(candidate)
}

func (session *RoomSession) Description() webrtc.SessionDescription {
	return *session.connection.LocalDescription()
}

func (session *RoomSession) Send(message any) error {
	val, err := json.Marshal(message)
	if err != nil {
		return err
	}

	return session.channel.Send(val)
}

func (session *RoomSession) Join(upload *UploadSession) error {
	for _, track := range upload.tracks {
		rtpSender, err := session.connection.AddTrack(track)
		if err != nil {
			return err
		}

		go forwardRtcp(session.sink, rtpSender, upload.connection)
	}

	return nil
}
