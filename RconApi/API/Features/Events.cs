using System;
using RconApi.API.EventsArgs;
using RconApi.API.Features.Clients;

namespace RconApi.API.Features
{
	public class Events
	{
		public event Action ServerStarting;
		public event Action ServerStarted;
		public event Action<ClientConnectedEventArgs> ClientConnected;
		public event Action<ClientDisconnectingEventArgs> ClientDisconnecting;
		public event Action<ClientDisconnectedEventArgs> ClientDisconnected;
		public event Action<ClientWriteEventArgs> ClientWrite;
		public event Action<UnknownPacketEventArgs> UnknownPacket;
		public event Action<Exception> ServerError;
		public event Action ServerStopping;
		public event Action ServerStopped;

		public void OnServerStarting() => ServerStarting?.Invoke();
		public void OnServerStarted() => ServerStarted?.Invoke();
		public void OnClientConnected(Client client) => ClientConnected?.Invoke(new(client));
		public void OnClientDisconnecting(Client client) => ClientDisconnecting?.Invoke(new(client));
		public void OnClientDisconnected(Client client) => ClientDisconnected?.Invoke(new(client));
		public void OnClientWrite(Client client) => ClientWrite?.Invoke(new (client));
		public void OnUnknownPacket(Client client) => UnknownPacket?.Invoke(new(client));
		public void OnServerError(Exception ex) => ServerError?.Invoke(ex);
		public void OnServerStopping() => ServerStopping?.Invoke();
		public void OnServerStopped() => ServerStopped?.Invoke();
	}
}