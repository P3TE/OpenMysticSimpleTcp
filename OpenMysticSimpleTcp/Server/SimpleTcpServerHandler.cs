using OpenMysticSimpleTcp.Multithreading;
using OpenMysticSimpleTcp.ReadWrite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OpenMysticSimpleTcp {
    public class SimpleTcpServerHandler : SimpleThreadHandler {

        private IPEndPoint ipEndPoint;

        private TcpListener tcpListener;

        private object connectedClientListLock = new object();

        private List<ServerConnectedClient> connectedClients = new List<ServerConnectedClient>();

        private int clientCount = 0;

        private ConcurrentQueue<StreamEventBase> receivedEventQueue = new ConcurrentQueue<StreamEventBase>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipEndPoint">Note: the construction of IPEndPoint checks for valid ports so we aren't doing any of those checks here.</param>
        public SimpleTcpServerHandler(IPEndPoint ipEndPoint) {
            this.ipEndPoint = ipEndPoint;
        }

        public void StartServer() {
            StartWorkerThread();
        }

        private void OnNewEvent(StreamEventBase newEvent) {
            receivedEventQueue.Enqueue(new EncapsulatedEvent<SimpleTcpServerHandler>(this, newEvent));
        }

        protected override void StopThreadSynchronous() {
            //Stop the connected clients.
            lock (connectedClientListLock) {
                foreach (ServerConnectedClient connectedClient in connectedClients) {
                    connectedClient.CloseConnectionSynchronous();
                }
                connectedClients.Clear();
            }
            tcpListener.Stop();
        }

        protected override void ThreadHandle(object threadParameter) {
            tcpListener = new TcpListener(ipEndPoint);

            while (ThreadRunning) {

                try {
                    Socket connectedClientSocket = tcpListener.AcceptSocket();

                    ServerConnectedClient serverConnectedClient = new ServerConnectedClient(connectedClientSocket, clientCount++);
                    OnNewClientConnected(serverConnectedClient);

                } catch (Exception e) {
                    if (ThreadRunning) {
                        //TODO - Something went wrong, notify of the error.
                        throw new NotImplementedException(e.Message);
                    }
                    return;
                }

            }

        }

        private void OnNewClientConnected(ServerConnectedClient serverConnectedClient) {
            lock (connectedClientListLock) {
                connectedClients.Add(serverConnectedClient);
            }
            serverConnectedClient.StartClientConnectionHandler();
        }
    }
}
