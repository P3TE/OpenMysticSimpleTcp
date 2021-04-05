using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace OpenMysticSimpleTcp.ReadWrite
{
    public class StreamReadHandler
    {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.
        private const int _DefaultBufferSize = 1024; //Bytes.
        byte[] receivedDataBuffer = new byte[_DefaultBufferSize];

        private readonly int threadJoinDuration;
        private readonly int bufferSize;

        private Stream dataStream;
        
        private Thread readThread;
        private bool threadRunning = false;

        private ConcurrentQueue<byte[]> receivedDataQueue = new ConcurrentQueue<byte[]>();

        public StreamReadHandler(Stream dataStream, int threadJoinDurationMilliseconds = _DefaultThreadJoinTimeoutMilliseconds)
        {

            this.dataStream = dataStream;
            this.threadJoinDuration = threadJoinDurationMilliseconds;
            
            threadRunning = true;
            readThread = new Thread(ReadThreadHandle)
            {
                IsBackground = true
            };
            readThread.Start();
        }

        private void ReadThreadHandle()
        {
            while (threadRunning)
            {
                

            }
        }


        private void Read()
        {
            int readAmount = dataStream.Read(receivedDataBuffer, 0, receivedDataBuffer.Length);
            if (readAmount < 0) {
                //Something bad happened!
                //TODO - Handle.
            } else {
                byte[] completeDataOnlyBuffer = new byte[readAmount];
                System.Buffer.BlockCopy(receivedDataBuffer, 0, completeDataOnlyBuffer, 0, readAmount);
                receivedDataQueue.Enqueue(completeDataOnlyBuffer);
            }
        }

        public bool GetLatestData(out byte[] data)
        {
            return receivedDataQueue.TryDequeue(out data);
        }

        public bool StopThread()
        {
            if (!threadRunning)
            {
                //We are already shutting down, ignore.
                return true;
            }
            
            try
            {
                dataStream.Close();
            }
            catch (Exception)
            {
                //We're closing anyway, ignore this exception.
            }

            bool threadShutdownSuccess = true;
            
            if (Thread.CurrentThread != readThread)
            {
                //If we are not the thread we are wanting to close, join the thread.
                threadShutdownSuccess = readThread.Join(threadJoinDuration);
            }

            return threadShutdownSuccess;
        }
        
    }
}