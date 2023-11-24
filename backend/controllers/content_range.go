package controllers

import (
	"fmt"
	"regexp"
	"strconv"

	"github.com/gin-gonic/gin"
	"github.com/go-http-utils/headers"
)

var contentRangeRegex = regexp.MustCompile(`^bytes (\d+)-(\d+)/(\d+)$`)

type contentRange = struct {
	startByte        int64
	endByte          int64
	rangeSize        int64
	rangeSizeBytes   int64
	contentSizeBytes int64
}

var errNoContentRange = fmt.Errorf("no Content-Range header was found")
var errInvalidFormat = fmt.Errorf("Content-Range header was in an invalid format")
var errInvalidRange = fmt.Errorf("Content-Range range needs to be bigger than 0")
var errInvalidSize = fmt.Errorf("Content-Range size needs to be bigger than 0")
var errInvalidEnd = fmt.Errorf("Content-Range end is bigger than content size")

func parseContentRange(c *gin.Context) (contentRange, error) {
	r := contentRange{}

	header := c.GetHeader(headers.ContentRange)

	if len(header) == 0 {
		return r, errNoContentRange
	}

	match := contentRangeRegex.FindStringSubmatch(header)

	if len(match) != 4 {
		return r, errInvalidFormat
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
		return r, errInvalidRange
	}

	if r.contentSizeBytes <= 0 {
		return r, errInvalidSize
	}

	if r.endByte >= r.contentSizeBytes {
		return r, errInvalidEnd
	}

	r.rangeSizeBytes = r.rangeSize + 1

	return r, nil
}
