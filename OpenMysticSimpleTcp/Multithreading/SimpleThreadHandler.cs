using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpenMysticSimpleTcp.Multithreading {
    public abstract class SimpleThreadHandler {

        private const int _DefaultThreadJoinTimeoutMilliseconds = 1000; //1 Second.

        private Thread workerThread;
        private volatile bool threadShutdownInProgress = false;
        private object threadShutdownLock = new object();

        private DelayedResult<bool> threadStopResult;

        private readonly int threadJoinTimeoutMilliseconds;

        private readonly bool isBackgroundThread;

        public bool ThreadRunning { get; private set; } = false;

        public SimpleThreadHandler(bool isBackground = true, int threadJoinTimeoutMilliseconds = _DefaultThreadJoinTimeoutMilliseconds) {

            this.isBackgroundThread = isBackground;
            this.threadJoinTimeoutMilliseconds = threadJoinTimeoutMilliseconds;

        }

        protected void StartWorkerThread(bool stopExistingThread = false) {

            if (ThreadRunning && !stopExistingThread) {
                throw new Exception("Existing thread is already running!");
            }

            DelayedResult<bool> stopThreadResult = StopThreadIfApplicableNohup();
            if (stopThreadResult.ExecutionComplete) {
                SpinupWorkerThread();
            } else {
                ThreadPool.QueueUserWorkItem(StartupThreadHandle, stopThreadResult);
            }

        }

        private void StartupThreadHandle(Object stateInfo) {
            //Try and stop the existing thread:
            DelayedResult<bool> stopThreadResult = (DelayedResult<bool>) stateInfo;
            stopThreadResult.WaitForExecutionCompleteion();
            //Start up the new thread.
            SpinupWorkerThread();
        }

        private void SpinupWorkerThread() {
            ThreadRunning = true;
            workerThread = new Thread(ThreadHandle) {
                IsBackground = isBackgroundThread
            };
            workerThread.Start();
        }

        public DelayedResult<bool> StopThreadIfApplicableNohup() {

            lock (threadShutdownLock) {

                if (threadShutdownInProgress) {
                    return threadStopResult;
                }

                threadStopResult = new DelayedResult<bool>();

                if (!ThreadRunning) {
                    //Already stopped.
                    threadStopResult.OnExecutionCompleted(true);
                    return threadStopResult;
                }

                ThreadRunning = false;
                threadShutdownInProgress = true;

            }

            ThreadPool.QueueUserWorkItem(StopThreadWorkerThreadHandle);

            return threadStopResult;
        }

        private void StopThreadWorkerThreadHandle(Object stateInfo) {
            bool threadShutdownSuccessfully = StopThreadSynchronous();
            threadStopResult.OnExecutionCompleted(threadShutdownSuccessfully);
            threadShutdownInProgress = false;
        }

        protected bool JoinThisThreadIfApplicable() {
            bool result = true;
            if (Thread.CurrentThread != workerThread) {
                result = workerThread.Join(threadJoinTimeoutMilliseconds);
            }
            return result;
        }

        protected abstract bool StopThreadSynchronous();

        protected abstract void ThreadHandle();

    }
}
