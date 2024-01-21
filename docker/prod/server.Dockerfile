# ==== Build backend
FROM golang:alpine AS backend

WORKDIR /streamfox

COPY backend/go.mod backend/go.sum ./
RUN go mod download

COPY backend .
RUN go build -o backend

# ==== Build frontend
FROM node:alpine AS frontend

WORKDIR /streamfox

COPY frontend/package.json frontend/package-lock.json ./
RUN npm i

COPY frontend .
RUN npm run build

# ==== Output prod env
FROM alpine:latest

WORKDIR /streamfox

RUN apk update && apk add --no-cache ffmpeg

COPY --from=backend /streamfox/backend /streamfox/backend
COPY --from=frontend /streamfox/frontend /streamfox/frontend
COPY backend/.env backend/logo_preview.png /streamfox/

EXPOSE 8801
CMD ["/streamfox/backend"]
