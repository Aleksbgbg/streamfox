package models

import (
	"database/sql/driver"
	"fmt"

	"github.com/bwmarrin/snowflake"
)

type Id snowflake.ID

const NilId Id = 0

var idGen *snowflake.Node

func setupIdGen() error {
	var err error
	idGen, err = snowflake.NewNode(1)
	return err
}

func (id Id) snowflake() snowflake.ID {
	return snowflake.ID(id)
}

func NewId() Id {
	return IdFromInt(idGen.Generate().Int64())
}

func IdFromString(value string) (Id, error) {
	id, err := snowflake.ParseBase58([]byte(value))
	return Id(id), err
}

func (id Id) String() string {
	return id.snowflake().Base58()
}

func IdFromInt(value int64) Id {
	return Id(snowflake.ParseInt64(value))
}

func (id Id) Int() int64 {
	return id.snowflake().Int64()
}

func (id *Id) Scan(value interface{}) error {
	idInt, ok := value.(int64)

	if !ok {
		return fmt.Errorf("unable to cast id to int: %#v", value)
	}

	*id = Id(snowflake.ParseInt64(idInt))
	return nil
}

func (id Id) Value() (driver.Value, error) {
	return id.snowflake().Int64(), nil
}
