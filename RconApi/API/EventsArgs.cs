using System;
using System.Net.Sockets;
using RconApi.API.Abstracts;
using RconApi.API.Features;

namespace RconApi.API.EventsArgs
{
	public class ClientConnectedEventArgs(TcpClient client) : AClient(client)
	{
	}

	public class ClientDisconnectingEventArgs(TcpClient client) : AClient(client)
	{
	}

	public class ClientWriteEventArgs<TEnumRequest>(ClientApi<TEnumRequest> clientApi) where TEnumRequest : Enum
	{
		public ClientApi<TEnumRequest> ClientApi { get; } = clientApi;
	}

	public class UnknownPacketEventArgs<TEnumRequest>(ClientApi<TEnumRequest> clientApi) where TEnumRequest : Enum
	{
		public ClientApi<TEnumRequest> ClientApi { get; } = clientApi;
	}
}