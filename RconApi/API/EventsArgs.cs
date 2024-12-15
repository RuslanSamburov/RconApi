using RconApi.API.Features.Clients;
using System;

namespace RconApi.API.EventsArgs
{
	public class ClientConnectedEventArgs(Client client)
	{
		public Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
		public DateTime ConnectedAt { get; } = client.ConnectedAt;
	}

	public class ClientDisconnectingEventArgs(Client client)
	{
		public Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
		public DateTime DisconnectingAt { get; } = DateTime.Now;
	}

	public class ClientDisconnectedEventArgs(Client client)
	{
		public Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
		public DateTime DisconnectedAt { get; } = DateTime.Now;
	}

	public class ClientWriteEventArgs(Client client)
	{
		public Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
		public ClientData Data => Client.Data;
	}

	public class UnknownPacketEventArgs(Client client)
	{
		public Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
		public int PacketType => Client.Data.PacketTypeRequest;
	}
}