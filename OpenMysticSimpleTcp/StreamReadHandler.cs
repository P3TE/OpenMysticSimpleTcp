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

        private ConcurrentQueue<StreamEventBase> receivedEventQueue;

        public StreamReadHandler(Stream dataStream, ConcurrentQueue<StreamEventBase> receivedEventQueue, int threadJoinDurationMilliseconds = _DefaultThreadJoinTimeoutMilliseconds)
        {

            this.dataStream = dataStream;
            this.receivedEventQueue = receivedEventQueue;
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
                try {
                    Read();
                } catch (Exception e) {
                    if (threadRunning) {
                        StreamConnectionEvent readThreadExceptionEvent = new StreamConnectionEvent(StreamConnectionEvent.StreamEventType.UnexpectedException, e);
                        receivedEventQueue.Enqueue(readThreadExceptionEvent);
                        StopThread();
                    }
                }
            }
        }

        private void Read()
        {
            int readAmount = dataStream.Read(receivedDataBuffer, 0, receivedDataBuffer.Length);
            if (readAmount < 0) {
                StreamConnectionEvent readThreadClosedEvent = new StreamConnectionEvent(StreamConnectionEvent.StreamEventType.ReadStreamClosedUnexpectedly);
                receivedEventQueue.Enqueue(readThreadClosedEvent);
                StopThread();
            } else {
                byte[] completeDataOnlyBuffer = new byte[readAmount];
                System.Buffer.BlockCopy(receivedDataBuffer, 0, completeDataOnlyBuffer, 0, readAmount);
                receivedEventQueue.Enqueue(new StreamDataReceivedEvent(completeDataOnlyBuffer));
            }
        }

        #region Thread Safe Functions

        public bool GetLatestData(out StreamEventBase latestEvent) {
            return receivedEventQueue.TryDequeue(out latestEvent);
        }

        public bool StopThread()
        {
            if (!threadRunning)
            {
                //We are already shutting down, ignore.
                return true;
            }
            threadRunning = false;


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

        #endregion

    }
}