using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMysticSimpleTcp.ReadWrite {
    public class StreamConnectionEvent : StreamEventBase {

        public enum StreamEventType {
            ReadStreamClosedUnexpectedly = 10,
            UnexpectedException = 20
        }

        private StreamEventType eventType;
        private Exception associatedException;

        public StreamConnectionEvent(StreamEventType streamEventType, Exception associatedException = null) {
            this.eventType = streamEventType;
            this.associatedException = associatedException;
        }

        public StreamEventType GetEventType() {
            return eventType;
        }

        public Exception GetAssociatedException() {
            return associatedException;
        }

        public void HandleEvent(IStreamEventHandler streamEventHandler) {
            streamEventHandler.OnStreamConnectionEvent(this);
        }
    }
}
