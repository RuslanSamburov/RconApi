using System.Collections.Generic;

namespace RconApi.API.Features.Clients
{
	public class ClientMetaData
	{
		public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

		public void SetMetadata(string key, object value)
		{
			Metadata[key] = value;
		}

		public T GetMetadata<T>(string key)
		{
			return Metadata.ContainsKey(key) ? (T)Metadata[key] : default;
		}
	}
}
