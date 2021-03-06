using OpenMysticSimpleTcp.Multithreading;
using OpenMysticSimpleTcp.ReadWrite;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenMysticSimpleTcp {
    public class SimpleTcpClientHandler : SimpleThreadHandler {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.

        public enum ClientConnectionState {
            Uninitialised = 0,
            AttemptingConnection = 10,
            Connected = 20,
            ConnectionClosed = 30
        }

        private ClientConnectionState clientConnectionState = ClientConnectionState.Uninitialised;

        private TcpClient tcpClient;

        private IPEndPoint connectionEndpoint;

        private StreamReadHandler streamReadHandler;
        private StreamWriteHandler streamWriteHandler;

        private ConcurrentQueue<StreamEventBase> receivedEventQueue = new ConcurrentQueue<StreamEventBase>();

        public SimpleTcpClientHandler() {
            
        }

        public void AttemptConnection(IPEndPoint endpoint) {

            this.connectionEndpoint = endpoint;
            this.clientConnectionState = ClientConnectionState.AttemptingConnection;
            base.StartWorkerThread();

        }

        public bool ConnectionInProgress {
            get {
                switch (clientConnectionState) {
                    case ClientConnectionState.Uninitialised:
                    case ClientConnectionState.ConnectionClosed:
                        return false;
                }
                return false;
            }
        }

        public bool TryDequeueNextEvent(out StreamEventBase streamEventBase) {
            return receivedEventQueue.TryDequeue(out streamEventBase);
        }

        protected override void ThreadHandle(object threadParameter) {
            CloseConnectionIfApplicable();

            try {
                clientConnectionState = ClientConnectionState.AttemptingConnection;
                tcpClient = new TcpClient();
                tcpClient.Connect(connectionEndpoint);

                NetworkStream networkStream = tcpClient.GetStream();
                streamReadHandler = new StreamReadHandler(networkStream, null);
                streamWriteHandler = new StreamWriteHandler(networkStream);

                clientConnectionState = ClientConnectionState.Connected;
            } catch (Exception e) {
                if (ConnectionInProgress) {
                    //TODO - An error occurred...
                }
                //TODO...
            }
        }

        protected override void StopThreadSynchronous() {
            CloseConnectionIfApplicable();
        }

        public void CloseConnectionIfApplicable() {

            if (!ConnectionInProgress) {
                //The connection is already closed.
                return;
            }

            clientConnectionState = ClientConnectionState.ConnectionClosed;

            try {
                tcpClient.Close();
            } catch (Exception) {
                //Ignore, we are attempting to close anyway.
            }

            try {
                streamReadHandler.StopThread();
            } catch (Exception) {
                //Ignore, we are attempting to close anyway.
            }

            try {
                streamWriteHandler.StopThread();
            } catch (Exception) {
                //Ignore, we are attempting to close anyway.
            }

        }

    }
}
