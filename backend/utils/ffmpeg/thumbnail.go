package ffmpeg

import (
	"fmt"
	"os"
	"os/exec"
	"strings"
)

func GenerateThumbnail(videoDir string) error {
	args := strings.Fields(fmt.Sprintf(
		"ffmpeg -loglevel error -y -i %s/video -vframes 1 -q:v 2 -vf scale=-1:225 -f mjpeg %s/thumbnail",
		videoDir,
		videoDir,
	))
	cmd := exec.Command(args[0], args[1:]...)
	cmd.Stderr = os.Stdout
	return cmd.Run()
}
