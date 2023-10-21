package codec

import (
	"context"
	"errors"
	"math"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"time"

	"gopkg.in/vansante/go-ffprobe.v2"
)

var formatToMimeType = map[string]string{
	"3dostr":                  "application/vnd.pg.format",
	"3g2":                     "video/3gpp2",
	"3gp":                     "video/3gpp",
	"4xm":                     "audio/x-adpcm",
	"a64":                     "application/octet-stream ",
	"aa":                      "application/octet-stream",
	"aac":                     "audio/aac",
	"ac3":                     "audio/x-ac3",
	"acm":                     "application/octet-stream",
	"adts":                    "audio/aac",
	"aiff":                    "audio/aiff",
	"amr":                     "audio/amr",
	"apng":                    "image/png",
	"asf":                     "video/x-ms-asf",
	"asf_stream":              "video/x-ms-asf",
	"ass":                     "text/x-ass",
	"au":                      "audio/basic",
	"avi":                     "video/x-msvideo",
	"avm2":                    "application/x-shockwave-flash",
	"bin":                     "application/octet-stream",
	"bit":                     "audio/bit",
	"caf":                     "audio/x-caf",
	"dts":                     "audio/x-dca",
	"dvd":                     "video/mpeg",
	"eac3":                    "audio/x-eac3",
	"f4v":                     "application/f4v",
	"flac":                    "audio/x-flac",
	"flv":                     "video/x-flv",
	"g722":                    "audio/G722",
	"g723_1":                  "audio/g723",
	"gif":                     "image/gif",
	"gsm":                     "audio/x-gsm",
	"h261":                    "video/x-h261",
	"h263":                    "video/x-h263",
	"hls":                     "application/x-mpegURL",
	"hls,applehttp":           "application/x-mpegURL",
	"ico":                     "image/vnd.microsoft.icon",
	"ilbc":                    "audio/iLBC",
	"ipod":                    "video/mp4",
	"ismv":                    "video/mp4",
	"jacosub":                 "text/x-jacosub",
	"jpeg_pipe":               "image/jpeg",
	"jpegls_pipe":             "image/jpeg",
	"latm":                    "audio/MP4A-LATM",
	"live_flv":                "video/x-flv",
	"m4v":                     "video/x-m4v",
	"matroska":                "video/x-matroska",
	"matroska,webm":           "video/webm",
	"microdvd":                "text/x-microdvd",
	"mjpeg":                   "video/x-mjpeg",
	"mjpeg_2000":              "video/x-mjpeg",
	"mmf":                     "application/vnd.smaf",
	"mov,mp4,m4a,3gp,3g2,mj2": "video/mp4",
	"mp2":                     "audio/mpeg",
	"mp3":                     "audio/mpeg",
	"mp4":                     "video/mp4",
	"mpeg":                    "video/mpeg",
	"mpeg1video":              "video/mpeg",
	"mpeg2video":              "video/mpeg",
	"mpegts":                  "video/MP2T",
	"mpegtsraw":               "video/MP2T",
	"mpegvideo":               "video/mpeg",
	"mpjpeg":                  "multipart/x-mixed-replace;boundary=ffserver",
	"mxf":                     "application/mxf",
	"mxf_d10":                 "application/mxf",
	"mxf_opatom":              "application/mxf",
	"nut":                     "video/x-nut",
	"oga":                     "audio/ogg",
	"ogg":                     "application/ogg",
	"ogv":                     "video/ogg",
	"oma":                     "audio/x-oma",
	"opus":                    "audio/ogg",
	"rm":                      "application/vnd.rn-realmedia",
	"singlejpeg":              "image/jpeg",
	"smjpeg":                  "image/jpeg",
	"spx":                     "audio/ogg",
	"srt":                     "application/x-subrip",
	"sup":                     "application/x-pgs",
	"svcd":                    "video/mpeg",
	"swf":                     "application/x-shockwave-flash",
	"tta":                     "audio/x-tta",
	"vcd":                     "video/mpeg",
	"vob":                     "video/mpeg",
	"voc":                     "audio/x-voc",
	"wav":                     "audio/x-wav",
	"webm":                    "video/webm",
	"webm_chunk":              "video/webm",
	"webm_dash_manifest":      "application/xml",
	"webp":                    "image/webp",
	"webvtt":                  "text/vtt",
	"wv":                      "audio/x-wavpack",
}

type ProbeResult struct {
	MimeType     string
	DurationSecs int32
}

var ErrInvalidVideoType = errors.New("invalid video type")

func Probe(videoId models.Id) (*ProbeResult, error) {
	file, err := files.NewResolver().AddVar(files.VideoId, videoId).Resolve(files.VideoStream)
	if err != nil {
		return nil, err
	}

	ctx, cancelFn := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelFn()

	data, err := ffprobe.ProbeURL(ctx, file.Path())
	if err != nil {
		return nil, err
	}

	formatName := data.Format.FormatName
	mimeType, ok := formatToMimeType[formatName]

	if !ok {
		return nil, ErrInvalidVideoType
	}

	return &ProbeResult{
		MimeType:     mimeType,
		DurationSecs: int32(math.Ceil(data.Format.DurationSeconds)),
	}, nil
}
