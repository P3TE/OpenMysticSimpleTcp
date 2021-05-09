using OpenMysticSimpleTcp.Multithreading;
using OpenMysticSimpleTcp.ReadWrite;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace OpenMysticSimpleTcp {
    public class ServerConnectedClient {

        public int UniqueId {
            get;
            private set;
        }

        private NetworkStream networkStream;

        private StreamReadHandler streamReadHandler;

        public ServerConnectedClient(Socket connectedClientSocket, int uniqueId) {
            this.networkStream = new NetworkStream(connectedClientSocket);
            this.streamReadHandler = new StreamReadHandler(networkStream, null);
            this.UniqueId = uniqueId;
        }

        public void StartClientConnectionHandler() {
            streamReadHandler.StartReadThread();
        }

        public void CloseConnectionSynchronous() {
            streamReadHandler.StopThreadIfApplicable();
        }

        public void SendData(byte[] buffer, int offset, int size) {
            networkStream.Write(buffer, offset, size);
        }

    }
}
