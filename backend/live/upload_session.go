package live

import (
	"errors"
	"fmt"
	"streamfox-backend/e"
	"streamfox-backend/models"
	"time"

	"github.com/pion/webrtc/v4"
)

var (
	errTrackTimeout = errors.New("could not receive media tracks within a reasonable amount of time")
)

var trackTimeout = time.Minute

var uploadTransceivers = [...]webrtc.RTPCodecType{
	webrtc.RTPCodecTypeVideo,
	webrtc.RTPCodecTypeAudio,
}

type UploadSession struct {
	connection *webrtc.PeerConnection

	tracks [len(uploadTransceivers)]*webrtc.TrackLocalStaticRTP

	sink *e.Sink

	uploadBegin chan struct{}
	exit        chan struct{}
}

func NewUploadSession(
	offer webrtc.SessionDescription,
	user *models.User,
) (*UploadSession, error) {
	api, err := newWebRtcApi()
	if err != nil {
		return nil, err
	}
	peerConnection, err := api.NewPeerConnection(configs)
	if err != nil {
		return nil, err
	}

	for _, codecType := range uploadTransceivers {
		if _, err = peerConnection.AddTransceiverFromKind(codecType); err != nil {
			return nil, err
		}
	}

	session := &UploadSession{
		connection:  peerConnection,
		tracks:      [len(uploadTransceivers)]*webrtc.TrackLocalStaticRTP{},
		sink:        e.NewSink(),
		uploadBegin: make(chan struct{}),
		exit:        make(chan struct{}),
	}

	tracks := make(chan *webrtc.TrackLocalStaticRTP)
	go func() {
		timeout := time.After(trackTimeout)
		errored := false

	loop:
		for i := 0; i < len(uploadTransceivers); i += 1 {
			select {
			case track := <-tracks:
				session.tracks[i] = track
			case <-timeout:
				errored = true
				session.sink.Fail(errTrackTimeout)
				break loop
			}
		}

		if !errored {
			session.uploadBegin <- struct{}{}
		}

		_, open := <-session.sink.Failed()
		if !open {
			return
		}

		session.sink.LogReport(
			fmt.Sprintf("Upload session failed for user %s(%s):", user.Name(), user.Id),
			e.LogLevelIgnored,
		)
		session.Close()
	}()

	peerConnection.OnTrack(func(remoteTrack *webrtc.TrackRemote, receiver *webrtc.RTPReceiver) {
		localTrack, err := webrtc.NewTrackLocalStaticRTP(
			remoteTrack.Codec().RTPCodecCapability,
			remoteTrack.ID(),
			remoteTrack.StreamID(),
		)
		if ok := session.sink.Check("could not create track", err, e.Silence(), e.Ignore()); !ok {
			return
		}

		tracks <- localTrack

		forwardRtp(session.sink, remoteTrack, localTrack)
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

func (session *UploadSession) Close() error {
	err := session.connection.Close()
	session.sink.Close()
	session.exit <- struct{}{}
	return err
}

func (session *UploadSession) Description() webrtc.SessionDescription {
	return *session.connection.LocalDescription()
}

func (session *UploadSession) UploadBegin() <-chan struct{} {
	return session.uploadBegin
}

func (session *UploadSession) Exit() <-chan struct{} {
	return session.exit
}
