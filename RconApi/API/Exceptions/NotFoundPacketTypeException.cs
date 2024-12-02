using System;

namespace RconApi.API.Exceptions
{
    public class NotFoundPacketTypeException(int packetInt) : Exception("Not found packet type")
    {
        public int PacketInt { get; } = packetInt;
    }
}
