using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OpenMysticSimpleTcp.Multithreading {
    public class DelayedResult <T> {

        private T result = default;

        private ManualResetEvent executionCompleteEvent = new ManualResetEvent(false);

        public void OnExecutionCompleted(T result) {
            this.result = result;
            executionCompleteEvent.Set();
        }

        public bool ExecutionComplete {
            get {
                return executionCompleteEvent.WaitOne(0);
            }
        }

        public T Result {
            get {
                return result;
            }
        }

        public bool WaitForExecutionCompleteion(int millisecondsTimeout = -1) {
            return executionCompleteEvent.WaitOne(millisecondsTimeout);
        }

    }
}
