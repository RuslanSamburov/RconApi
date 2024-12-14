using System;
using System.Net.Sockets;
using RconApi.API.Abstracts;

namespace RconApi.API.Features
{
	public class ClientApi<TEnumRequest> : AClient where TEnumRequest : Enum
	{
		public ClientData<TEnumRequest> ClientData { get; }

		public ClientApi(TcpClient client) : base(client)
		{
			ClientData = new ClientData<TEnumRequest>(BinaryReader);
		}
	}
}