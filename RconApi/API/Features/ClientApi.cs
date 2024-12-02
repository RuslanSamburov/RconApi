using System;
using System.IO;
using System.Net.Sockets;

namespace RconApi.API.Features
{
    public class ClientApi<TEnumRequest>(TcpClient client) : IDisposable where TEnumRequest : Enum
    {
        public TcpClient Client { get; } = client;
        public ClientData<TEnumRequest> ClientData { get; } = new();
        public BinaryWriter BinaryWriter { get; } = new(client.GetStream());
        public BinaryReader BinaryReader { get; } = new(client.GetStream());

        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed) return;

            BinaryReader?.Dispose();
            BinaryWriter?.Dispose();

            Client?.Close();

            _disposed = true;
        }
    }
}
