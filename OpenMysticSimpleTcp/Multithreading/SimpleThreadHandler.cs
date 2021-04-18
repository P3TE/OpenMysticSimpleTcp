using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpenMysticSimpleTcp.Multithreading {
    public abstract class SimpleThreadHandler {

        public enum ThreadState {
            Uninitialised,
            Running,
            Stopping,
            Stopped
        }

        private Thread workerThread;
        private object threadShutdownLock = new object();

        private readonly bool isBackgroundThread;

        public ThreadState CurrentThreadState {
            get;
            private set;
        } = ThreadState.Uninitialised;

        public bool ThreadRunning {
            get {
                return CurrentThreadState == ThreadState.Running;
            }
        }

        public SimpleThreadHandler(bool isBackground = true) {

            this.isBackgroundThread = isBackground;

        }

        protected void StartWorkerThread(bool stopExistingThread = false, Object threadParameter = null) {

            if (ThreadRunning && !stopExistingThread) {
                throw new Exception("Existing thread is already running!");
            }

            if (stopExistingThread) {
                StopThreadIfApplicable();
            }

            SpinupWorkerThread(threadParameter);

        }

        private void SpinupWorkerThread(Object threadParameter) {
            CurrentThreadState = ThreadState.Running;
            workerThread = new Thread(ThreadHandle) {
                IsBackground = isBackgroundThread
            };
            workerThread.Start(threadParameter);
        }

        public void StopThreadIfApplicable() {

            lock (threadShutdownLock) {

                if (!ThreadRunning) {
                    //Already stopped.
                    return;
                }

                CurrentThreadState = ThreadState.Stopping;
                StopThreadSynchronous();
                CurrentThreadState = ThreadState.Stopped;

            }

        }

        /// <summary>
        /// A quick note on joining the thread:
        /// Before calling this function, think about whether you actually need it.
        /// I personally used to think it was important that I wait for 
        /// any and every thread to stop when I had finished with it.
        /// However, now that I think about it, most of the time, it's not really required.
        /// In most cases, just calling stop to release the resources used by the thread
        /// and letting the thread finish of it's own accord.
        /// </summary>
        /// <param name="threadJoinTimeoutMilliseconds"></param>
        /// <returns></returns>
        public virtual bool JoinThisThreadIfApplicable(int threadJoinTimeoutMilliseconds = 0) {
            bool result = true;
            if (Thread.CurrentThread != workerThread) {
                result = workerThread.Join(threadJoinTimeoutMilliseconds);
            }
            return result;
        }

        protected abstract bool StopThreadSynchronous();

        protected abstract void ThreadHandle(Object threadParameter);

    }
}
