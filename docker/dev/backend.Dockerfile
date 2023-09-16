FROM golang:alpine

WORKDIR /backend

COPY go.mod go.sum ./
RUN go mod download
RUN go install github.com/silenceper/gowatch@latest

RUN apk update && apk add --no-cache ffmpeg

EXPOSE 8701
CMD ["gowatch"]
