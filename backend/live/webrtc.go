package live

import (
	"io"
	"streamfox-backend/config"
	"streamfox-backend/e"

	"github.com/pion/ice/v3"
	"github.com/pion/interceptor"
	"github.com/pion/webrtc/v4"
)

const rtpBufSizeBytes = 4096

var (
	configs  webrtc.Configuration
	settings webrtc.SettingEngine
)

func SetupWebRtc() error {
	mux, err := ice.NewMultiUDPMuxFromPort(config.Values.LiveUdpPort)
	if err != nil {
		return err
	}
	settings.SetICEUDPMux(mux)
	settings.SetNAT1To1IPs([]string{config.Values.LivePublicIp}, webrtc.ICECandidateTypeHost)

	return nil
}

func newWebRtcApi() (*webrtc.API, error) {
	engine := &webrtc.MediaEngine{}
	for _, codec := range [...]webrtc.RTPCodecParameters{
		{
			RTPCodecCapability: webrtc.RTPCodecCapability{
				MimeType:    webrtc.MimeTypeH264,
				ClockRate:   90000,
				Channels:    0,
				SDPFmtpLine: "level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42001f",
				RTCPFeedback: []webrtc.RTCPFeedback{
					{Type: "goog-remb", Parameter: ""},
					{Type: "ccm", Parameter: "fir"},
					{Type: "nack", Parameter: ""},
					{Type: "nack", Parameter: "pli"},
				},
			},
			PayloadType: 102,
		},
	} {
		if err := engine.RegisterCodec(codec, webrtc.RTPCodecTypeVideo); err != nil {
			return nil, err
		}
	}
	for _, codec := range [...]webrtc.RTPCodecParameters{
		{
			RTPCodecCapability: webrtc.RTPCodecCapability{
				MimeType:     webrtc.MimeTypeOpus,
				ClockRate:    48000,
				Channels:     2,
				SDPFmtpLine:  "minptime=10;useinbandfec=1",
				RTCPFeedback: nil,
			},
			PayloadType: 111,
		},
	} {
		if err := engine.RegisterCodec(codec, webrtc.RTPCodecTypeAudio); err != nil {
			return nil, err
		}
	}

	interceptors := &interceptor.Registry{}
	if err := webrtc.RegisterDefaultInterceptors(engine, interceptors); err != nil {
		return nil, err
	}

	return webrtc.NewAPI(
		webrtc.WithSettingEngine(settings),
		webrtc.WithMediaEngine(engine),
		webrtc.WithInterceptorRegistry(interceptors),
	), nil
}

func forwardRtp(sink *e.Sink, src *webrtc.TrackRemote, dst *webrtc.TrackLocalStaticRTP) {
	rtpBuf := make([]byte, rtpBufSizeBytes)
	for {
		bytesRead, _, err := src.Read(rtpBuf)
		if ok := sink.Check("could not read RTP packets", err, e.Silence(io.EOF), e.Ignore()); !ok {
			return
		}

		_, err = dst.Write(rtpBuf[:bytesRead])
		if ok := sink.Check("could not forward RTP packets", err, e.Silence(), e.Ignore(io.ErrClosedPipe)); !ok {
			return
		}
	}
}

func forwardRtcp(sink *e.Sink, src *webrtc.RTPSender, dst *webrtc.PeerConnection) {
	for {
		rtcpPackets, _, err := src.ReadRTCP()
		if ok := sink.Check("could not read RTCP packets", err, e.Silence(io.EOF), e.Ignore()); !ok {
			return
		}

		err = dst.WriteRTCP(rtcpPackets)
		if ok := sink.Check("could not forward RTCP packets", err, e.Silence(io.ErrClosedPipe), e.Ignore()); !ok {
			return
		}
	}
}
