using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace OpenMysticSimpleTcp.ReadWrite {
    public class StreamWriteHandler {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.

        private readonly int threadJoinDuration;

        private Stream dataStream;

        private Thread writeThread;
        private bool threadRunning = false;

        private ConcurrentQueue<ByteArraySubsection> dataToSend = new ConcurrentQueue<ByteArraySubsection>();

        private ConcurrentQueue<StreamEventBase> receivedEventQueue = new ConcurrentQueue<StreamEventBase>();

        private ManualResetEvent waitHandle = new ManualResetEvent(false);

        public StreamWriteHandler(Stream dataStream, int threadJoinDurationMilliseconds = _DefaultThreadJoinTimeoutMilliseconds) {
            
            this.dataStream = dataStream;
            this.threadJoinDuration = threadJoinDurationMilliseconds;

            threadRunning = true;
            writeThread = new Thread(WriteThreadHandle) {
                IsBackground = true
            };
            writeThread.Start();
        }

        private void WriteThreadHandle() {
            while (threadRunning) {
                try {

                    waitHandle.WaitOne();

                    if (!threadRunning) {
                        break;
                    }

                    waitHandle.Reset();

                    SendAllWaitingData();

                } catch (Exception e) {
                    if (threadRunning) {
                        StreamConnectionEvent readThreadExceptionEvent = new StreamConnectionEvent(StreamConnectionEvent.StreamEventType.UnexpectedException, e);
                        receivedEventQueue.Enqueue(readThreadExceptionEvent);
                        StopThread();
                    }
                }
            }
        }

        private void SendAllWaitingData() {
            while (dataToSend.TryDequeue(out ByteArraySubsection bufferToWrite)) {
                dataStream.Write(bufferToWrite.data, bufferToWrite.offset, bufferToWrite.count);
            }
        }

        #region Thread Safe Functions

        public void QueueDataToSend(ByteArraySubsection data) {
            dataToSend.Enqueue(data);
        }

        public bool GetLatestData(out StreamEventBase latestEvent) {
            return receivedEventQueue.TryDequeue(out latestEvent);
        }

        public bool StopThread() {
            if (!threadRunning) {
                //We are already shutting down, ignore.
                return true;
            }
            threadRunning = false;

            try {
                waitHandle.Set();
            } catch (Exception) {
                //We're closing anyway, ignore this exception.
            }

            try {
                dataStream.Close();
            } catch (Exception) {
                //We're closing anyway, ignore this exception.
            }

            bool threadShutdownSuccess = true;

            if (Thread.CurrentThread != writeThread) {
                //If we are not the thread we are wanting to close, join the thread.
                threadShutdownSuccess = writeThread.Join(threadJoinDuration);
            }

            return threadShutdownSuccess;
        }

        #endregion
    }
}