package controllers

import (
	"fmt"
	"net/http"
	"streamfox-backend/utils"
	"strings"

	"github.com/abrander/ginproxy"
	"github.com/gin-gonic/gin"
)

func ProxyFrontendMiddleware(apiPrefix string) gin.HandlerFunc {
	g, _ := ginproxy.NewGinProxy(
		fmt.Sprintf(
			"http://%s:%s",
			utils.GetEnvVar(utils.DEBUG_FORWARD_HOST),
			utils.GetEnvVar(utils.DEBUG_FORWARD_PORT),
		),
	)
	return func(c *gin.Context) {
		c.Next()

		if (c.Writer.Status() == http.StatusNotFound) &&
			!strings.Contains(c.Request.URL.Path, apiPrefix) {
			g.Handler(c)
		}
	}
}
