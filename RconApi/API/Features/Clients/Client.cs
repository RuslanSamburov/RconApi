using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RconApi.API.Features.Clients
{
	public class Client : IDisposable
	{
		private bool _disposed;

		private Rcon Rcon { get; }
		private TcpClient TcpClient { get; }

		public Guid Guid { get; }
		public ClientData Data { get; }
		public ClientMetaData MetaData { get; } = new();

		public string IP => (TcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();
		public int Port => (TcpClient.Client.RemoteEndPoint as IPEndPoint)?.Port ?? 0;
		public DateTime ConnectedAt { get; }
		public bool Connected => TcpClient.Connected;

		public BinaryReader BinaryReader { get; }
		public BinaryWriter BinaryWriter { get; }
		public NetworkStream Stream { get; }

		internal Client(ref TcpClient client, Rcon rcon)
		{
			TcpClient = client;
			Stream = client.GetStream();
			BinaryReader = new BinaryReader(Stream);
			BinaryWriter = new BinaryWriter(Stream);

			Rcon = rcon;
			Data = new ClientData(BinaryReader);
			Guid = Guid.NewGuid();
			ConnectedAt = DateTime.Now;
		}

		public async Task SendResponse(string message, int type = 0, int? messageId = null)
		{
			if (!Connected)
				throw new InvalidOperationException("Cannot send response to a disconnected client.");

			messageId ??= Data.MessageId;
			await SendResponse(BinaryWriter, (int)messageId, type, message);
		}

		public static async Task SendResponse(BinaryWriter writer, int messageId, int type, string message)
		{
			byte[] payload = Encoding.UTF8.GetBytes(message);
			writer.Write(payload.Length + 10);
			writer.Write(messageId);
			writer.Write(Convert.ToInt32(type));
			writer.Write(payload);
			writer.Write((short)0);
			await writer.BaseStream.FlushAsync();
		}

		public void Disconnect()
		{
			Rcon.Events.OnClientDisconnecting(this);

			BinaryReader?.Dispose();
			BinaryWriter?.Dispose();
			Stream?.Dispose();
			TcpClient?.Close();

			Rcon.Events.OnClientDisconnected(this);
		}

		public void Dispose()
		{
			if (_disposed) return;

			try
			{
				Disconnect();
			}
			finally
			{
				_disposed = true;
			}
		}
	}
}