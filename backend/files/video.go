package files

import (
	"log"
	"os"
	"path/filepath"
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

func VideoPath(file VideoFile, id string) string {
	folder := filepath.Join(root, videoBase, id)
	os.Mkdir(folder, defaultPerm)
	return filepath.Join(folder, videoFileString(file))
}

func VideoHandle(file VideoFile, id string) (*os.File, string, error) {
	path := VideoPath(file, id)
	handle, err := os.OpenFile(path, os.O_CREATE, defaultPerm)
	return handle, path, err
}
