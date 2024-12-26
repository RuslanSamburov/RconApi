using RconApi.API.Features;
using RconApi.API.Features.Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RconApi.API
{
	public class Rcon(int port, IPAddress ipAddress)
	{
		public static readonly Version Version = new(1, 0, 3);
		private TcpListener _listener;

		private readonly List<Client> _clients = [];

		public int Port { get; } = port;
		public IPAddress IPAddress { get; } = ipAddress;

		public Events Events { get; } = new();

		public Dictionary<int, Func<Client, Task>> _requestFuncs { get; } = [];
		public Dictionary<Type, Func<Client, Exception, Task>> _exceptions { get; } = [];

		private CancellationTokenSource _cancellationTokenSource;

		public void StartServer()
		{
			_cancellationTokenSource = new CancellationTokenSource();

			Task.Run(async () =>
			{
				try
				{
					_listener = new TcpListener(IPAddress, Port);
					Events.OnServerStarting();
					_listener.Start();
					Events.OnServerStarted();

					while (!IsCancellationRequested())
					{
						TcpClient client = await _listener.AcceptTcpClientAsync();

						Client clientApi = new(ref client, this);

						_clients.Add(clientApi);

						Events.OnClientConnected(clientApi);
						_ = HandleClient(clientApi);
					}
				}
				catch (Exception ex)
				{
					Events.OnServerError(ex);
				}
			});
		}

		public void StopServer()
		{
			Events.OnServerStopping();
			_clients.ForEach(x => x.Disconnect());
			_listener.Stop();
			_cancellationTokenSource.Cancel();
			Events.OnServerStopped();
		}

		public IReadOnlyList<Client> GetClients()
		{
			return _clients.AsReadOnly();
		}

		public bool IsCancellationRequested()
		{
			return _cancellationTokenSource.Token.IsCancellationRequested;
		}

		private async Task HandleClient(Client client)
		{
			while (client.Connected)
			{
				try
				{
					client.Data.Update();
				}
				catch
				{
					client.Disconnect();
					break;
				}

				try
				{
					Events.OnClientWrite(client);

					if (_requestFuncs.TryGetValue(client.Data.PacketTypeRequest, out Func<Client, Task> func))
					{
						await func(client);
					}
					else
					{
						Events.OnUnknownPacket(client);
					}
				}
				catch (ArgumentNullException)
				{
					Events.OnUnknownPacket(client);
				}
				catch (Exception ex)
				{
					if (_exceptions.TryGetValue(ex.GetType(), out Func<Client, Exception, Task> func))
					{
						await func(client, ex);
					}
					else
					{
						Events.OnServerError(ex);
					}
				}
			}
			_clients.Remove(client);
		}
	}
}