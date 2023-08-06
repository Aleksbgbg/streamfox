package files

import (
	"log"
	"os"
	"path/filepath"
	"streamfox-backend/models"
)

type VideoFile int

const (
	Stream VideoFile = iota
	Thumbnail
)

const videoBase = "videos"

func videoFileString(file VideoFile) string {
	switch file {
	case Stream:
		return "video"
	case Thumbnail:
		return "thumbnail"
	}

	log.Panicf("Invalid VideoFile %d.\n", file)
	return ""
}

func VideoPath(file VideoFile, videoId models.Id) string {
	folder := filepath.Join(root, videoBase, videoId.String())
	os.Mkdir(folder, defaultPerm)
	return filepath.Join(folder, videoFileString(file))
}

func VideoHandle(file VideoFile, videoId models.Id) (*os.File, string, error) {
	path := VideoPath(file, videoId)
	handle, err := os.OpenFile(path, os.O_CREATE, defaultPerm)
	return handle, path, err
}
