using RconApi.API.Features.Clients;

namespace RconAuth.EventsArgs
{
	public class AuthenticatedEventArgs(Client client)
	{
		public Client Client { get; } = client;
	}

	public class NotAuthenticatedEventArgs(Client client)
	{
		public Client Client { get; } = client;
	}

	public class CommandEventArgs(Client client)
	{
		public Client Client { get; } = client;
		public string Payload => Client.Data.Payload;
	}
}
