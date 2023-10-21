package codec

import (
	"fmt"
	"math"
	"os"
	"os/exec"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"strings"
)

func GenerateThumbnail(videoId models.Id, probe *ProbeResult) error {
	resolver := files.NewResolver().AddVar(files.VideoId, videoId)

	stream, err := resolver.Resolve(files.VideoStream)
	if err != nil {
		return err
	}

	thumbnail, err := resolver.Resolve(files.VideoThumbnail)
	if err != nil {
		return err
	}

	const seekFraction = 0.2
	args := strings.Fields(
		fmt.Sprintf(
			"ffmpeg -loglevel error -y -ss %d -i %s -vframes 1 -q:v 2 -vf scale=416:234:force_original_aspect_ratio=decrease,pad=416:234:-1:-1:color=black -f mjpeg %s",
			int(math.Floor(float64(probe.DurationSecs)*seekFraction)),
			stream.Path(),
			thumbnail.Path(),
		),
	)
	cmd := exec.Command(args[0], args[1:]...)
	cmd.Stderr = os.Stdout
	return cmd.Run()
}
