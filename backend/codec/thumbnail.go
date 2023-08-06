package codec

import (
	"fmt"
	"os"
	"os/exec"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"strings"
)

func GenerateThumbnail(videoId models.Id) error {
	args := strings.Fields(fmt.Sprintf(
		"ffmpeg -loglevel error -y -i %s -vframes 1 -q:v 2 -vf scale=-1:225 -f mjpeg %s",
		files.VideoPath(files.Stream, videoId),
		files.VideoPath(files.Thumbnail, videoId),
	))
	cmd := exec.Command(args[0], args[1:]...)
	cmd.Stderr = os.Stdout
	return cmd.Run()
}
