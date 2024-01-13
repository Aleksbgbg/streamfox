package models

import (
	"time"
)

type Base struct {
	Id Id `gorm:"primaryKey; autoIncrement:false"`
	RowMetadata
}

type RowMetadata struct {
	CreatedAt time.Time `gorm:"not null"`
	UpdatedAt time.Time `gorm:"not null"`
}
