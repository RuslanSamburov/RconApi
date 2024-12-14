using System.IO;
using System.Net.Sockets;

namespace RconApi.API.Abstracts
{
	public class AClient
	{
		public TcpClient TcpClient { get; }
		public BinaryWriter BinaryWriter { get; }
		public BinaryReader BinaryReader { get; }

		public AClient(TcpClient client)
		{
			TcpClient = client;
			var stream = client.GetStream();
			BinaryWriter = new BinaryWriter(stream);
			BinaryReader = new BinaryReader(stream);
		}
	}
}
