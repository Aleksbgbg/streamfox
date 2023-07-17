package models

import (
	"time"

	"gorm.io/gorm"
)

type Base struct {
	Id        int64 `gorm:"primaryKey; autoIncrement:false"`
	CreatedAt time.Time
	UpdatedAt time.Time
	DeletedAt gorm.DeletedAt `gorm:"index"`
}
