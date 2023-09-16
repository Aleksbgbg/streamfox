# ==== Build server
FROM golang:alpine

WORKDIR /streamfox
COPY backend .
RUN go build -o backend

# ==== Build client
FROM node:alpine

WORKDIR /streamfox
COPY frontend .
RUN npm i
RUN npm run build -- --outDir frontend

# ==== Prod env
FROM alpine:latest

WORKDIR /streamfox

RUN apk update && apk add --no-cache ffmpeg

COPY --from=0 /streamfox/backend /streamfox/backend
COPY --from=1 /streamfox/frontend /streamfox/frontend
COPY backend/.env backend/logo_preview.png /streamfox/

EXPOSE 8801
CMD ["/streamfox/backend"]
