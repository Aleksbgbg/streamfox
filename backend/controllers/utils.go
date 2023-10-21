package controllers

import (
	"github.com/gin-gonic/gin"
)

func hasContentLength(c *gin.Context) bool {
	return (c.Request.ContentLength > 0) ||
		((c.Request.ContentLength == 0) && (c.Request.Body == nil))
}
