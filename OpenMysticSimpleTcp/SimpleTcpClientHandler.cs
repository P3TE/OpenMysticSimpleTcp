using OpenMysticSimpleTcp.Multithreading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenMysticSimpleTcp.ReadWrite {
    public class SimpleTcpClientHandler {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.

        public enum ClientConnectionState {
            Uninitialised = 0,
            AttemptingConnection = 10,
            Connected = 20,
            ConnectionClosed = 30
        }

        private ClientConnectionState clientConnectionState = ClientConnectionState.Uninitialised;

        private readonly int threadJoinDuration;

        private TcpClient tcpClient;

        private Thread tcpConnectionThread;

        private IPEndPoint connectionEndpoint;

        private StreamReadHandler streamReadHandler;
        private StreamWriteHandler streamWriteHandler;

        public SimpleTcpClientHandler(int threadJoinDurationMilliseconds = _DefaultThreadJoinTimeoutMilliseconds) {
            this.threadJoinDuration = threadJoinDurationMilliseconds;
        }

        public void AttemptConnection(IPEndPoint endpoint) {

            this.connectionEndpoint = endpoint;
            this.clientConnectionState = ClientConnectionState.AttemptingConnection;

            tcpConnectionThread = new Thread(ConnectThreadHandle) {
                IsBackground = true
            };
            tcpConnectionThread.Start();

        }

        private void ConnectThreadHandle() {

            CloseConnectionIfApplicable();

            try {
                clientConnectionState = ClientConnectionState.AttemptingConnection;
                tcpClient = new TcpClient();
                tcpClient.Connect(connectionEndpoint);
                
                NetworkStream networkStream = tcpClient.GetStream();
                streamReadHandler = new StreamReadHandler(networkStream, this.threadJoinDuration);
                streamWriteHandler = new StreamWriteHandler(networkStream, this.threadJoinDuration);

                clientConnectionState = ClientConnectionState.Connected;
            } catch (Exception e) {
                if (ConnectionInProgress) {
                    //TODO - An error occurred...
                }
                //TODO...
            }
            
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

        public DelayedResult<bool> CloseConnectionIfApplicable() {

            DelayedResult<bool> result = new DelayedResult<bool>();

            if (!ConnectionInProgress) {
                //The connection is already closed.
                result.OnExecutionCompleted(true);
                return result;
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

            bool threadShutdownSuccess = true;

            if (Thread.CurrentThread != tcpConnectionThread) {
                //If we are not the thread we are wanting to close, join the thread.
                threadShutdownSuccess = tcpConnectionThread.Join(threadJoinDuration);
            }

            return result;

        }

    }
}
