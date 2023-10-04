package files

import (
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

func ResolvePathNone(key string) (string, error) {
	return fs.ResolvePath(key, VarMap{})
}

func ResolvePathSingle(key, varKey, varValue string) (string, error) {
	return fs.ResolvePath(key, VarMap{varKey: varValue})
}

func ResolveFileSingle(key, varKey, varValue string) (*os.File, string, error) {
	return fs.ResolveFile(key, VarMap{varKey: varValue})
}
