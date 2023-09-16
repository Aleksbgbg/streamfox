package files

import (
	"os"
	"path/filepath"
	"streamfox-backend/utils"
)

const defaultPerm = os.ModePerm

var root string

func Setup() {
	root = utils.GetEnvVar(utils.APP_DATA_ROOT)
	os.Mkdir(root, defaultPerm)

	for _, subdir := range [...]string{videoBase} {
		os.Mkdir(filepath.Join(root, subdir), defaultPerm)
	}
}
