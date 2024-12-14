using RconApi.API.Features;
using RconAuth.EventsArgs;
using System;

namespace RconAuth.Features
{
	public class EventsAuth<TEnumRequest> where TEnumRequest : Enum
	{
		public event Action<AuthenticatedEventArgs<TEnumRequest>> Authenticated;
		public event Action<NotAuthenticatedEventArgs<TEnumRequest>> NotAuthenticated;
		public event Action<CommandEventArgs<TEnumRequest>> Command;

		public void OnAuthenticated(ClientApi<TEnumRequest> clientApi)
		{
			Authenticated?.Invoke(new(clientApi));
		}

		public void OnNotAuthenticated(ClientApi<TEnumRequest> clientApi)
		{
			NotAuthenticated?.Invoke(new(clientApi));
		}

		public void OnCommand(ClientApi<TEnumRequest> clientApi)
		{
			NotAuthenticated?.Invoke(new(clientApi));
		}
	}
}
