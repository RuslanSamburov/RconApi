using RconApi.API.Features.Clients;
using RconAuth.EventsArgs;
using System;

namespace RconAuth.Features
{
	public class EventsAuth
	{
		public event Action<AuthenticatedEventArgs> Authenticated;
		public event Action<NotAuthenticatedEventArgs> NotAuthenticated;
		public event Action<CommandEventArgs> Command;

		public void OnAuthenticated(Client client)
		{
			Authenticated?.Invoke(new(client));
		}

		public void OnNotAuthenticated(Client client)
		{
			NotAuthenticated?.Invoke(new(client));
		}

		public void OnCommand(Client client)
		{
			Command?.Invoke(new(client));
		}
	}
}
