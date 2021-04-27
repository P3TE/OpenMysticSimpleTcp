using System;

namespace OpenMysticSimpleTcp.ReadWrite {
	public abstract class StreamEventBase {

		public abstract void HandleEvent(IStreamEventHandler streamEventHandler);

	}
}