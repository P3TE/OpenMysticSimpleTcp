using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMysticSimpleTcp.ReadWrite {
    public class EncapsulatedEvent<T> : StreamEventBase {

        public T Encapsulator {
            private set;
            get;
        }

        public StreamEventBase BaseEvent {
            private set;
            get;
        }

        public EncapsulatedEvent(T encapsulator, StreamEventBase baseEvent){
            this.Encapsulator = encapsulator;
            this.BaseEvent = baseEvent;
        }

        public override string ToString() {
            return $"{Encapsulator.GetType().Name}: {BaseEvent.ToString()}";
        }
    }
}
