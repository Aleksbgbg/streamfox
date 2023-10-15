package models

type Subtitle struct {
	Base

	VideoId Id `gorm:"not null"`
	Video   Video

	Name string `gorm:"not null; type:text"`
}

func CreateSubtitle(id Id, videoId Id, Name string) (*Subtitle, error) {
	subtitle := &Subtitle{
		Base:    Base{Id: id},
		VideoId: videoId,
		Name:    Name,
	}
	err := db.Create(subtitle).Error
	return subtitle, err
}

func GetAllSubtitles(video Id) ([]Subtitle, error) {
	var subtitles []Subtitle
	err := db.Order("name").Find(&subtitles, &Subtitle{VideoId: video}).Error
	return subtitles, err
}

func GetSubtitle(id Id) (*Subtitle, error) {
	subtitle := &Subtitle{}
	err := db.Find(subtitle, &Subtitle{Base: Base{Id: id}}).Error
	return subtitle, err
}

func (subtitle *Subtitle) Save() error {
	return db.Save(subtitle).Error
}

func (subtitle *Subtitle) Delete() error {
	return db.Delete(&Subtitle{Base: Base{Id: subtitle.Id}}).Error
}
