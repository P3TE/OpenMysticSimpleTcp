using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMysticSimpleTcp.ReadWrite {
    public interface IStreamEventHandler {

        void OnStreamConnectionEvent(StreamConnectionEvent streamConnectionEvent);

        void OnDataReceivedEvent(StreamDataReceivedEvent streamDataReceivedEvent);

    }
}
