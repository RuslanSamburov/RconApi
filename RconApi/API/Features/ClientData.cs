using System;
using System.IO;
using System.Text;

namespace RconApi.API.Features
{
	public class ClientData<TEnumRequest>(BinaryReader reader) where TEnumRequest : Enum
	{
		private readonly BinaryReader _reader = reader;

		public int Length { get; private set; }
		public int MessageId { get; private set; }
		public int PacketTypeRequest { get; private set; }
		public string Payload { get; private set; }

		public void Update()
		{
			Length = _reader.ReadInt32();
			MessageId = _reader.ReadInt32();
			PacketTypeRequest = _reader.ReadInt32();
			Payload = Encoding.UTF8.GetString(_reader.ReadBytes(Length - 10));
			_reader.ReadInt16();
		}

		public TEnumRequest GetPacketType()
		{
			return (TEnumRequest)Enum.ToObject(typeof(TEnumRequest), PacketTypeRequest);
		}
	}
}