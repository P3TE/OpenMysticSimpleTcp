using OpenMysticSimpleTcp.Multithreading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace OpenMysticSimpleTcp.ReadWrite
{
    public class StreamReadHandler : SimpleThreadHandler
    {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.
        private const int _DefaultBufferSize = 1024; //Bytes.
        byte[] receivedDataBuffer = new byte[_DefaultBufferSize];

        private readonly int bufferSize;

        private Stream dataStream;

        private ConcurrentQueue<StreamEventBase> receivedEventQueue;

        public StreamReadHandler(Stream dataStream, ConcurrentQueue<StreamEventBase> receivedEventQueue)
        {

            this.dataStream = dataStream;
            this.receivedEventQueue = receivedEventQueue;
            
        }

        public void StartReadThread() {
            StartWorkerThread();
        }

        private void Read()
        {
            int readAmount = dataStream.Read(receivedDataBuffer, 0, receivedDataBuffer.Length);
            if (readAmount < 0) {
                StreamConnectionEvent readThreadClosedEvent = new StreamConnectionEvent(StreamConnectionEvent.StreamEventType.ReadStreamClosedUnexpectedly);
                receivedEventQueue.Enqueue(readThreadClosedEvent);
                StopThreadIfApplicable();
            } else {
                byte[] completeDataOnlyBuffer = new byte[readAmount];
                System.Buffer.BlockCopy(receivedDataBuffer, 0, completeDataOnlyBuffer, 0, readAmount);
                receivedEventQueue.Enqueue(new StreamDataReceivedEvent(completeDataOnlyBuffer));
            }
        }

        protected override void ThreadHandle(object threadParameter) {
            while (ThreadRunning) {
                try {
                    Read();
                } catch (Exception e) {
                    if (ThreadRunning) {
                        StreamConnectionEvent readThreadExceptionEvent = new StreamConnectionEvent(StreamConnectionEvent.StreamEventType.UnexpectedException, e);
                        receivedEventQueue.Enqueue(readThreadExceptionEvent);
                        StopThreadIfApplicable();
                    }
                }
            }
        }

        #region Thread Safe Functions

        public bool GetLatestData(out StreamEventBase latestEvent) {
            return receivedEventQueue.TryDequeue(out latestEvent);
        }

        protected override void StopThreadSynchronous() {
            try {
                dataStream.Close();
            } catch (Exception) {
                //We're closing anyway, ignore this exception.
            }
        }       

        #endregion

    }
}