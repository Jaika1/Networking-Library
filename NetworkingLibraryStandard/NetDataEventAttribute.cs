using System;
using System.Collections.Generic;
using System.Text;

namespace Jaika1.Networking
{
    public class NetDataEventAttribute : Attribute
    {
        private byte eventId;
        private int eventGroupIdentifier;

        public byte EventId => eventId;
        public int EventGroupIdentifier => eventGroupIdentifier;

        public NetDataEventAttribute(byte eventId, int eventGroupIdentifier = 0)
        {
            this.eventId = eventId;
            this.eventGroupIdentifier = eventGroupIdentifier;
        }
    }
}
