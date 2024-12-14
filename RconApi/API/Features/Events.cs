using System;
using System.Net.Sockets;
using RconApi.API.EventsArgs;

namespace RconApi.API.Features
{
	public class Events<TEnumRequest> where TEnumRequest : Enum
	{
		public event Action ServerStarting;
		public event Action ServerStarted;
		public event Action<ClientConnectedEventArgs> ClientConnected;
		public event Action<ClientDisconnectingEventArgs> ClientDisconnecting;
		public event Action<ClientWriteEventArgs<TEnumRequest>> ClientWrite;
		public event Action<UnknownPacketEventArgs<TEnumRequest>> UnknownPacket;
		public event Action<Exception> ServerError;
		public event Action ServerStopping;
		public event Action ServerStopped;

		public void OnServerStarting() => ServerStarting?.Invoke();
		public void OnServerStarted() => ServerStarted?.Invoke();
		public void OnClientConnected(TcpClient client) => ClientConnected?.Invoke(new(client));
		public void OnClientDisconnecting(TcpClient client) => ClientDisconnecting?.Invoke(new(client));
		public void OnClientWrite(ClientApi<TEnumRequest> client) => ClientWrite?.Invoke(new (client));
		public void OnUnknownPacket(ClientApi<TEnumRequest> client) => UnknownPacket?.Invoke(new(client));
		public void OnServerError(Exception ex) => ServerError?.Invoke(ex);
		public void OnServerStopping() => ServerStopping?.Invoke();
		public void OnServerStopped() => ServerStopped?.Invoke();
	}
}