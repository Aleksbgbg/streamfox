FROM golang:alpine

WORKDIR /backend

RUN apk update && apk add --no-cache ffmpeg

COPY go.mod go.sum ./
RUN go mod download

RUN go install github.com/silenceper/gowatch@latest

EXPOSE 8701
CMD ["gowatch"]
