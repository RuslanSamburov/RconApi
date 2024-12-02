using RconApi.API.Exceptions;
using System;
using System.IO;
using System.Text;

namespace RconApi.API.Features
{
    public class ClientData<TEnumRequest> where TEnumRequest : Enum
    {
        public int Length { get; private set; }
        public int MessageId { get; private set; }
        public int PacketTypeInt { get; private set; }
        public TEnumRequest PacketTypeRequest { get; private set; }
        public string Payload { get; private set; }

        internal void Update(BinaryReader reader)
        {
            Length = reader.ReadInt32();

            if (Length < 10)
            {
                throw new InvalidDataException("Packet length is too short.");
            }

            MessageId = reader.ReadInt32();
            PacketTypeInt = reader.ReadInt32();
            try
            {
                PacketTypeRequest = (TEnumRequest)Enum.ToObject(typeof(TEnumRequest), PacketTypeInt);
            } catch (ArgumentException)
            {
                throw new NotFoundPacketTypeException(PacketTypeInt);
            }
            Payload = Encoding.UTF8.GetString(reader.ReadBytes(Length - 10));
            reader.ReadInt16();
        }
    }
}
