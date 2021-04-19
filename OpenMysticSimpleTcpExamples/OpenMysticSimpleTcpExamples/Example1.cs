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
            ThreadPool.QueueUserWorkItem(ExampleClientThreadHandle);

            //Run the server:

            IPEndPoint serverListenEndPoint = new IPEndPoint(IPAddress.Any, serverPort);


            Console.WriteLine("Shutting down.");

            Thread.Sleep(2000);

        }


        private static void ExampleClientThreadHandle(object parameter) {

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, serverPort);

            SimpleTcpClientHandler simpleTcpClientHandler = new SimpleTcpClientHandler();
        }

    }
}
