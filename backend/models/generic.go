package models

func count[T any](value *T) (int64, error) {
	var count int64
	err := db.Model(new(T)).
		Where(value).
		Count(&count).Error
	return count, err
}
