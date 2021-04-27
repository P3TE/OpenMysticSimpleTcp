using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenMysticSimpleTcp;
using OpenMysticSimpleTcp.Multithreading;
using OpenMysticSimpleTcp.ReadWrite;

namespace OpenMysticSimpleTcpExamples {
    class Example1 {

        const int serverPort = 8888;

        static void Main(string[] args) {

            //Start the client in a separate thread.
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, serverPort);
            ExampleTcpClient exampleTcpClient = new ExampleTcpClient(serverEndPoint);

            //Run the server:

            ServerHandle();

            Console.WriteLine("Threads completed, press any key to exit.");

            Console.ReadLine();

        }

        private class ExampleTcpClient : IStreamEventHandler {

            private SimpleTcpClientHandler simpleTcpClientHandler;
            private IPEndPoint serverEndPoint;

            public ExampleTcpClient(IPEndPoint serverEndPoint) {
                this.serverEndPoint = serverEndPoint;
                ThreadPool.QueueUserWorkItem(ClientThreadHandle);
            }

            private void ClientThreadHandle(object parameter) {

                simpleTcpClientHandler = new SimpleTcpClientHandler();

                //Attempt to connect to the server.
                simpleTcpClientHandler.AttemptConnection(serverEndPoint);

                //The Update loop.
                while (true) {

                    //Handle the latest events.
                    while (simpleTcpClientHandler.TryDequeueNextEvent(out StreamEventBase nextEvent)){
                        try {
                            nextEvent.HandleEvent(this);
                        } catch (Exception e) {
                            //Exceptions can be caught to allow processing of later events without the exception propogating above this scope.
                            Console.WriteLine($"Caught exception with message: {e.Message}");
                        }
                    }

                    //Run at 10Hz.
                    Thread.Sleep(100);
                }

            }

            #region IStreamEventHandler Implementation

            public void OnDataReceivedEvent(StreamDataReceivedEvent streamDataReceivedEvent) {
                throw new NotImplementedException();
            }

            public void OnStreamConnectionEvent(StreamConnectionEvent streamConnectionEvent) {
                throw new NotImplementedException();
            }

            #endregion

        }


        private static void ServerHandle() {

            IPEndPoint serverListenEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
            SimpleTcpServerHandler simpleTcpServerHandler = new SimpleTcpServerHandler(serverListenEndPoint);

            while (true) {

                simpleTcpServerHandler.HandleUpdates();

                //Run at 10Hz.
                Thread.Sleep(100);
            }
        }


    }
}
