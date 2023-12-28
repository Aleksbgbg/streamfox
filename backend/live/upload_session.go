package live

import (
	"errors"
	"fmt"
	"log"
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
	tracks     [len(uploadTransceivers)]*webrtc.TrackLocalStaticRTP
	sink       *e.Sink
}

func NewUploadSession(
	offer webrtc.SessionDescription,
	user *models.User,
	begin func(*UploadSession),
	end func(*UploadSession),
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
		connection: peerConnection,
		tracks:     [len(uploadTransceivers)]*webrtc.TrackLocalStaticRTP{},
		sink:       e.NewSink(),
	}

	tracks := make(chan *webrtc.TrackLocalStaticRTP)
	go func() {
		timeout := time.NewTicker(trackTimeout)
		timedOut := false
		log.Println("Stort")

	loop:
		for i := 0; i < len(uploadTransceivers); i += 1 {
			log.Println("Loop")
			select {
			case track := <-tracks:
				log.Println("Get Tracc")
				session.tracks[i] = track
			case <-timeout.C:
				timedOut = true
				session.sink.Fail(errTrackTimeout)
				break loop
			}
		}

		log.Println("Break loop")

		if !timedOut {
			timeout.Stop()
			begin(session)

			log.Println("Schedule")
			defer func() {
				end(session)
				log.Println("End me!")
			}()
		}
		log.Println("Nat")

		_, open := <-session.sink.Failed()
		if !open {
			log.Println("End!!!")
			return
		}

		session.sink.LogReport(
			fmt.Sprintf("Upload session failed for user %s(%s):", user.Name(), user.Id),
			e.LogLevelIgnored,
		)
		log.Println("End2!!!")
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
	return err
}

func (session *UploadSession) Description() webrtc.SessionDescription {
	return *session.connection.LocalDescription()
}
