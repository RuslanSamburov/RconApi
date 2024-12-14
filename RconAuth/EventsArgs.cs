using RconApi.API.Features;
using System;

namespace RconAuth.EventsArgs
{
	public class AuthenticatedEventArgs<TEnumRequest>(ClientApi<TEnumRequest> clientApi) where TEnumRequest : Enum
	{
		public ClientApi<TEnumRequest> ClientApi { get; } = clientApi;
	}

	public class NotAuthenticatedEventArgs<TEnumRequest>(ClientApi<TEnumRequest> clientApi) where TEnumRequest : Enum
	{
		public ClientApi<TEnumRequest> ClientApi { get; } = clientApi;
	}

	public class CommandEventArgs<TEnumRequest>(ClientApi<TEnumRequest> clientApi) where TEnumRequest : Enum
	{
		public ClientApi<TEnumRequest> ClientApi { get; } = clientApi;
	}
}
