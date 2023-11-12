package codec

import (
	"context"
	"fmt"
	"os"
	"os/exec"
	"streamfox-backend/files"
	"streamfox-backend/models"
	"strings"
	"time"

	"gopkg.in/vansante/go-ffprobe.v2"
)

type Subtitle struct {
	Id    models.Id
	Title string
}

func extractSubtitleFile(videoPath string, subtitlePath string, subTrackId int) error {
	args := strings.Fields(
		fmt.Sprintf(
			"ffmpeg -loglevel error -y -i %s -map 0:s:%d %s",
			videoPath,
			subTrackId,
			subtitlePath,
		),
	)
	cmd := exec.Command(args[0], args[1:]...)
	cmd.Stderr = os.Stdout
	return cmd.Run()
}

func ExtractAllSubtitles(videoId models.Id) ([]Subtitle, error) {
	resolver := files.NewResolver().AddVar(files.VideoId, videoId)

	videoFile, err := resolver.Resolve(files.VideoStream)
	if err != nil {
		return nil, err
	}

	ctx, cancelFn := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelFn()

	data, err := ffprobe.ProbeURL(ctx, videoFile.Path())
	if err != nil {
		return nil, err
	}

	var subs []Subtitle
	subTrackId := 0

	for _, s := range data.Streams {
		if s.CodecType != "subtitle" {
			continue
		}

		subtitle := Subtitle{}
		subtitle.Id = models.NewId()
		if val, err := s.TagList.GetString("title"); err == nil {
			subtitle.Title = val
		} else {
			subtitle.Title = fmt.Sprintf("Track %d", subTrackId+1)
		}

		subtitleFile, err := resolver.AddVar(files.SubtitleId, subtitle.Id).Resolve(files.VideoSubtitle)
		if err != nil {
			return nil, err
		}

		err = extractSubtitleFile(videoFile.Path(), subtitleFile.Path(), subTrackId)
		if err != nil {
			return nil, err
		}

		subs = append(subs, subtitle)
		subTrackId += 1
	}

	return subs, nil
}

func convertSubtitleFile(srcPath, dstPath string) error {
	// dstPath should already include the .vtt extension
	args := strings.Fields(fmt.Sprintf("ffmpeg -loglevel error -y -i %s %s", srcPath, dstPath))
	cmd := exec.Command(args[0], args[1:]...)
	cmd.Stderr = os.Stdout
	return cmd.Run()
}

func ConvertToVtt(videoId models.Id, subtitleId models.Id) error {
	resolver := files.NewResolver().
		AddVar(files.VideoId, videoId).
		AddVar(files.SubtitleId, subtitleId)

	src, err := resolver.Resolve(files.VideoSubtitleTemp)
	if err != nil {
		return err
	}

	dst, err := resolver.Resolve(files.VideoSubtitle)
	if err != nil {
		return err
	}

	return convertSubtitleFile(src.Path(), dst.Path())
}
