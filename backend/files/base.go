package files

import (
	"fmt"
	"os"
	"streamfox-backend/config"
)

const fsTree = `
<config_root>
	auth_secret #auth_secret
	streamfox_default_password #streamfox_default_password
<data_root>
	videos
		<video_id>
			stream #video_stream
			thumbnail #video_thumbnail
			subtitles
				<subtitle_id>.vtt #video_subtitle
				<subtitle_id>.temp.unknown #video_subtitle_temp
`

const (
	ConfigRoot = "config_root"
	DataRoot   = "data_root"
	VideoId    = "video_id"
	SubtitleId = "subtitle_id"
)

const (
	AuthSecret               = "auth_secret"
	StreamfoxDefaultPassword = "streamfox_default_password"
	VideoStream              = "video_stream"
	VideoThumbnail           = "video_thumbnail"
	VideoSubtitle            = "video_subtitle"
	VideoSubtitleTemp        = "video_subtitle_temp"
)

var fs = ParseFsTree(fsTree)

func Setup() {
	fs.AddVar(ConfigRoot, config.Values.AppConfigRoot)
	fs.AddVar(DataRoot, config.Values.AppDataRoot)
}

type Resolver struct {
	fs   *Fs
	vars VarMap
}

func NewResolver() *Resolver {
	return &Resolver{fs: &fs, vars: VarMap{}}
}

func (r *Resolver) AddVar(key string, value any) *Resolver {
	r.vars[key] = fmt.Sprint(value)
	return r
}

func (r *Resolver) Resolve(key string) (*File, error) {
	path, err := r.fs.ResolvePath(key, r.vars)
	if err != nil {
		return nil, err
	}

	return &File{path: path}, nil
}

type File struct {
	path string
	file *os.File
}

func (f *File) Path() string {
	return f.path
}

func (f *File) Open() (*os.File, error) {
	file, err := os.OpenFile(f.path, os.O_CREATE|os.O_RDWR, DefaultPerm)
	if err != nil {
		return nil, err
	}

	f.file = file

	return file, nil
}

func (f *File) AutoClose() {
	if f.file == nil {
		return
	}

	f.file.Close()
	f.file = nil
}

func (f *File) Remove() error {
	f.AutoClose()
	return os.Remove(f.path)
}

func (f *File) ReadOrFillIfEmpty(filler func() string) (string, error) {
	content, err := os.ReadFile(f.Path())
	if err != nil {
		return "", err
	}

	if len(content) == 0 {
		content = []byte(filler())
		err = os.WriteFile(f.Path(), content, DefaultPerm)
		if err != nil {
			return "", err
		}
	}

	return string(content), nil
}
