using System;
using System.Collections.Generic;
using System.Text;

namespace OpenMysticSimpleTcp {
    public class ByteArraySubsection {

        public byte[] data;
        public int offset;
        public int count;

        public ByteArraySubsection(byte[] data, int offset = -1, int length = -1) {

            this.data = data;

            if (offset < 0) {
                this.offset = 0;
            } else {
                this.offset = offset;
            }

            if (length < 0) {
                this.count = data.Length - offset;
            } else {
                this.count = length;
            }

            if ((offset + length) > data.Length) {
                throw new IndexOutOfRangeException("Array offset and length exceed array size!");
            }
            
        }
    }
}
