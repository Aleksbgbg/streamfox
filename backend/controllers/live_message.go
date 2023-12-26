package controllers

type messageType int

const (
	messageConnected messageType = iota
	messageDisconnected
	messageStartStream
	messageStopStream
)

type message struct {
	Type    messageType `json:"type"`
	Payload interface{} `json:"payload"`
}

type payloadConnected struct {
	User UserInfo `json:"user"`
}

type payloadDisconnected struct {
	User UserInfo `json:"user"`
}

type payloadStartStream struct {
	UserId string `json:"userId"`
}

type payloadStopStream struct {
	UserId string `json:"userId"`
}

func newMessageConnected(user UserInfo) message {
	return message{
		Type: messageConnected,
		Payload: payloadConnected{
			User: user,
		},
	}
}

func newMessageDisconnected(user UserInfo) message {
	return message{
		Type: messageDisconnected,
		Payload: payloadDisconnected{
			User: user,
		},
	}
}

func newMessageStartStream(id string) message {
	return message{
		Type: messageStartStream,
		Payload: payloadStartStream{
			UserId: id,
		},
	}
}

func newMessageStopStream(id string) message {
	return message{
		Type: messageStopStream,
		Payload: payloadStopStream{
			UserId: id,
		},
	}
}
