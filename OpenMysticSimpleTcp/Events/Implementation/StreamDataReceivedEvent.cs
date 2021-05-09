using System;

namespace OpenMysticSimpleTcp.ReadWrite {
	public class StreamDataReceivedEvent : StreamEventBase {

		private byte[] receivedData;

		public StreamDataReceivedEvent(byte[] receivedData) {
			this.receivedData = receivedData;
		}

		public byte[] GetReceivedData() {
			return receivedData;
		}

		public void HandleEvent(IStreamEventHandler streamEventHandler) {
			streamEventHandler.OnDataReceivedEvent(this);
		}
	}
}