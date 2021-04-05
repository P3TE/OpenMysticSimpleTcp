using System;

namespace OpenMysticSimpleTcp.ReadWrite {
	public abstract class StreamDataReceivedEvent : StreamEventBase {

		private byte[] receivedData;

		public StreamDataReceivedEvent(byte[] receivedData) {
			this.receivedData = receivedData;
		}

		public byte[] GetReceivedData() {
			return receivedData;
		}

	}
}