using RconApi.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RconApi.API
{
	public class Rcon<TEnumResponse, TEnumRequest>(int port, IPAddress ipAddress) where TEnumResponse : Enum where TEnumRequest : Enum
	{
		public static readonly Version Version = new(1, 0, 2);
		private TcpListener _listener;

		public int Port { get; } = port;
		public IPAddress IPAddress { get; } = ipAddress;
		public Events<TEnumRequest> Events { get; } = new();
		public Dictionary<TEnumRequest, Func<ClientApi<TEnumRequest>, Task>> RequestFuncs { get; } = [];
		public Dictionary<Type, Func<ClientApi<TEnumRequest>, Exception, Task>> Exceptions { get; } = [];
		private CancellationTokenSource _cancellationTokenSource;

		public void StartServer()
		{
			_cancellationTokenSource = new CancellationTokenSource();

			Task.Run(() =>
			{
				try
				{
					_listener = new TcpListener(IPAddress, Port);
					Events.OnServerStarting();
					_listener.Start();
					Events.OnServerStarted();

					while (!IsCancellationRequested())
					{
						TcpClient client = _listener.AcceptTcpClient();
						Events.OnClientConnected(client);
						_ = HandleClient(client);
					}
				}
				catch (Exception ex)
				{
					Events.OnServerError(ex);
				}
			});
		}

		public void ClientClose(TcpClient client)
		{
			Events.OnClientDisconnecting(client);
			client.Close();
		}

		public void StopServer()
		{
			Events.OnServerStopping();
			_listener.Stop();
			_cancellationTokenSource.Cancel();
			Events.OnServerStopped();
		}

		public static async Task SendResponse(BinaryWriter writer, int messageId, Enum type, string message)
		{
			byte[] payload = Encoding.UTF8.GetBytes(message);
			writer.Write(payload.Length + 10);
			writer.Write(messageId);
			writer.Write(Convert.ToInt32(type));
			writer.Write(payload);
			writer.Write((short)0);
			await writer.BaseStream.FlushAsync();
		}

		public bool IsCancellationRequested()
		{
			return _cancellationTokenSource.Token.IsCancellationRequested;
		}

		private async Task HandleClient(TcpClient client)
		{
			ClientApi<TEnumRequest> clientApi = new(client);

			while (clientApi.TcpClient.Connected)
			{
				clientApi.ClientData.Update();
				try
				{
					Events.OnClientWrite(clientApi);
					TEnumRequest packetType = clientApi.ClientData.GetPacketType();

					if (RequestFuncs.TryGetValue(packetType, out Func<ClientApi<TEnumRequest>, Task> func))
					{
						await func(clientApi);
					} else
					{
						Events.OnUnknownPacket(clientApi);
					}
				}
				catch (ArgumentNullException)
				{
					Events.OnUnknownPacket(clientApi);
				}
				catch (Exception ex)
				{
					if (Exceptions.TryGetValue(ex.GetType(), out var func))
					{
						await func(clientApi, ex);
					}
					else
					{
						Events.OnServerError(ex);
					}
				}
			}
		}
	}
}