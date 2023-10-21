package files

import (
	"fmt"
	"os"
	"streamfox-backend/utils"
)

const fsTree = `
<config_root>
	auth_secret #auth_secret
<data_root>
	videos
		<video_id>
			stream #video_stream
			thumbnail #video_thumbnail
`

const (
	ConfigRoot = "config_root"
	DataRoot   = "data_root"
	VideoId    = "video_id"
)

const (
	AuthSecret     = "auth_secret"
	VideoStream    = "video_stream"
	VideoThumbnail = "video_thumbnail"
)

var fs = ParseFsTree(fsTree)

func Setup() {
	fs.AddVar(ConfigRoot, utils.GetEnvVar(utils.APP_CONFIG_ROOT))
	fs.AddVar(DataRoot, utils.GetEnvVar(utils.APP_DATA_ROOT))
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