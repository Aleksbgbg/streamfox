package controllers

import (
	"fmt"
	"regexp"
	"strconv"

	"github.com/gin-gonic/gin"
)

var contentRangeRegex = regexp.MustCompile(`^bytes (\d+)-(\d+)/(\d+)$`)

type contentRange = struct {
	startByte        int64
	endByte          int64
	rangeSize        int64
	rangeSizeBytes   int64
	contentSizeBytes int64
}

var noContentRange = fmt.Errorf("no Content-Range header was found")
var invalidFormat = fmt.Errorf("Content-Range header was in an invalid format")
var invalidRange = fmt.Errorf("Content-Range range needs to be bigger than 0")
var invalidSize = fmt.Errorf("Content-Range size needs to be bigger than 0")
var invalidEnd = fmt.Errorf("Content-Range end is bigger than content size")

func parseContentRange(c *gin.Context) (contentRange, error) {
	r := contentRange{}

	header := c.GetHeader("Content-Range")

	if len(header) == 0 {
		return r, noContentRange
	}

	match := contentRangeRegex.FindStringSubmatch(header)

	if len(match) != 4 {
		return r, invalidFormat
	}

	start := match[1]
	end := match[2]
	size := match[3]

	var err error

	r.startByte, err = strconv.ParseInt(start, 10, 64)
	if err != nil {
		return r, err
	}

	r.endByte, err = strconv.ParseInt(end, 10, 64)
	if err != nil {
		return r, err
	}

	r.contentSizeBytes, err = strconv.ParseInt(size, 10, 64)
	if err != nil {
		return r, err
	}

	r.rangeSize = r.endByte - r.startByte
	if r.rangeSize < 0 {
		return r, invalidRange
	}

	if r.contentSizeBytes <= 0 {
		return r, invalidSize
	}

	if r.endByte >= r.contentSizeBytes {
		return r, invalidEnd
	}

	r.rangeSizeBytes = r.rangeSize + 1

	return r, nil
}
