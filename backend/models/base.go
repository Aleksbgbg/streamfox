package models

import (
	"time"
)

type Base struct {
	Id        int64 `gorm:"primaryKey; autoIncrement:false"`
	CreatedAt time.Time
	UpdatedAt time.Time
}
