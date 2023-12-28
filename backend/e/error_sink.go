package e

import (
	"errors"
	"fmt"
	"log"
	"strings"
	"sync"
)

type Sink struct {
	ignored  []error
	silenced []error
	fatal    []error

	failed chan struct{}

	lock sync.Mutex
}

func NewSink() *Sink {
	return &Sink{
		ignored:  nil,
		silenced: nil,
		fatal:    nil,
		failed:   make(chan struct{}, 1),
	}
}

func (s *Sink) Failed() <-chan struct{} {
	return s.failed
}

func (s *Sink) Close() {
	close(s.failed)
}

func (s *Sink) Clear() {
	clear(s.ignored)
	clear(s.silenced)
	clear(s.fatal)
}

func (s *Sink) Fail(err error) {
	s.lock.Lock()
	s.fatal = append(s.fatal, err)
	s.lock.Unlock()

	s.failed <- struct{}{}
}

type SilencedErrs []error

func Silence(errs ...error) SilencedErrs {
	return errs
}

type IgnoredErrs []error

func Ignore(errs ...error) IgnoredErrs {
	return errs
}

func (s *Sink) Check(message string, err error, silenced SilencedErrs, ignored IgnoredErrs) bool {
	if err == nil {
		return true
	}

	err = fmt.Errorf("%s: %w", message, err)

	for _, e := range ignored {
		if errors.Is(err, e) {
			s.lock.Lock()
			s.ignored = append(s.ignored, err)
			s.lock.Unlock()
			return true
		}
	}

	for _, e := range silenced {
		if errors.Is(err, e) {
			s.lock.Lock()
			s.silenced = append(s.silenced, err)
			s.lock.Unlock()
			return false
		}
	}

	s.Fail(err)
	return false
}

type LogLevel int

const (
	LogLevelFatal LogLevel = iota
	LogLevelSilenced
	LogLevelIgnored
)

type LogStage struct {
	name  string
	errs  []error
	level LogLevel
}

func (s *Sink) LogReport(header string, level LogLevel) {
	builder := strings.Builder{}
	builder.WriteString(fmt.Sprintf("! Error report\n%s\n", header))

	s.lock.Lock()
	for _, stage := range [...]LogStage{
		{"Fatal", s.fatal, LogLevelFatal},
		{"Silenced", s.silenced, LogLevelSilenced},
		{"Ignored", s.ignored, LogLevelIgnored},
	} {
		if level < stage.level {
			break
		}

		if len(stage.errs) == 0 {
			continue
		}

		builder.WriteString(fmt.Sprintf("\t%s (%d):\n", stage.name, len(stage.errs)))
		for _, e := range stage.errs {
			builder.WriteString(fmt.Sprintf("\t\t> %+v\n", e))
		}
	}
	s.lock.Unlock()

	log.Print(builder.String())
}
