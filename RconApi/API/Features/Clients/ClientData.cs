using System.IO;
using System.Text;

namespace RconApi.API.Features.Clients
{
	public class ClientData(BinaryReader reader)
	{
		public int Length { get; private set; }
		public int MessageId { get; private set; }
		public int PacketTypeRequest { get; private set; }
		public string Payload { get; private set; }

		internal void Update()
		{
			Length = reader.ReadInt32();
			MessageId = reader.ReadInt32();
			PacketTypeRequest = reader.ReadInt32();
			Payload = Encoding.UTF8.GetString(reader.ReadBytes(Length - 10));
			reader.ReadInt16();
		}
	}
}