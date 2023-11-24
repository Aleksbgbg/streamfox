package controllers

import (
	"bufio"
	"bytes"
	"fmt"
	"io"
	"net"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/go-http-utils/headers"
)

type deferredResponseWriter struct {
	context  *gin.Context
	response gin.ResponseWriter
	body     *bytes.Buffer
	status   int
	flushed  bool
}

func (w *deferredResponseWriter) Header() http.Header {
	return w.response.Header()
}

func (w *deferredResponseWriter) WriteHeader(status int) {
	w.status = status
}

func (w *deferredResponseWriter) Write(buf []byte) (int, error) {
	w.body.Write(buf)
	return len(buf), nil
}

func (w *deferredResponseWriter) Hijack() (net.Conn, *bufio.ReadWriter, error) {
	return w.response.Hijack()
}

func (w *deferredResponseWriter) Flush() {
	if w.flushed {
		fmt.Printf("Duplicate flush, ignoring\n")
		return
	}

	w.response.WriteHeader(w.status)
	if w.body.Len() > 0 {
		w.response.Header().Set(headers.ContentLength, fmt.Sprint(w.body.Len()))
		_, err := io.Copy(w.response, w.body)
		if err != nil {
			w.context.Error(err)
			return
		}
		w.body.Reset()
	}
	w.flushed = true
}

func (w *deferredResponseWriter) CloseNotify() <-chan bool {
	return w.response.CloseNotify()
}

func (w *deferredResponseWriter) Status() int {
	return w.status
}

func (w *deferredResponseWriter) Size() int {
	return w.body.Len()
}

func (w *deferredResponseWriter) WriteString(str string) (int, error) {
	return w.Write([]byte(str))
}

func (w *deferredResponseWriter) Written() bool {
	return w.flushed
}

func (w *deferredResponseWriter) WriteHeaderNow() {
	fmt.Printf("WriteHeaderNow requested, ignoring\n")
}

func (w *deferredResponseWriter) Pusher() http.Pusher {
	return w.response.Pusher()
}
