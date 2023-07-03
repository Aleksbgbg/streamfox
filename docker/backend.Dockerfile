FROM golang:1.20-alpine

WORKDIR /backend
COPY go.mod go.sum ./
RUN go mod download
RUN go install github.com/silenceper/gowatch@latest

RUN apk update && apk add --no-cache ffmpeg

EXPOSE 5000

CMD ["gowatch"]
