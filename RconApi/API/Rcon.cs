using RconApi.API.Exceptions;
using RconApi.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RconApi.API
{
    public class Rcon<TEnumResponse, TEnumRequest>(int port, IPAddress ipAddress) where TEnumResponse : Enum where TEnumRequest : Enum
    {
        public static readonly Version Version = new(1, 0, 0);

        private TcpListener _listener { get; set; }

        public int Port { get; } = port;
        public IPAddress IPAddress { get; } = ipAddress;

        public event Action ServerStarting;
        public event Action ServerStarted;

        public event Action<ClientApi<TEnumRequest>> ClientConnected;

        public event Action<ClientApi<TEnumRequest>> ClientDisconnecting;

        public event Action<ClientApi<TEnumRequest>> UnknownPacket;

        public event Action<Exception> ServerError;

        public event Action ServerStoping;
        public event Action ServerStoped;

        public Dictionary<TEnumRequest, Func<ClientApi<TEnumRequest>, Task>> RequestFuncs = [];
        public Dictionary<Type, Func<ClientApi<TEnumRequest>, Exception, Task>> Exceptions = [];

        private CancellationTokenSource _cancellationTokenSource;

        public void StartServer()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                try
                {
                    _listener = new(IPAddress, Port);
                    ServerStarting?.Invoke();
                    _listener.Start();
                    ServerStarted?.Invoke();

                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        ClientApi<TEnumRequest> ClientApi = new(client);

                        ClientConnected?.Invoke(ClientApi);
                        _ = HandleClient(ClientApi);
                    }
                }
                catch (Exception ex)
                {
                    ServerError?.Invoke(ex);
                }
            });
        }

        public void ClientClose(ClientApi<TEnumRequest> client)
        {
            ClientDisconnecting?.Invoke(client);
            client.Client.Close();
        }

        public void StopServer()
        {
            ServerStoping?.Invoke();
            _listener.Stop();

            _cancellationTokenSource.Cancel();
            ServerStoped?.Invoke();
        }

        public static async Task SendResponse(BinaryWriter writer, int messageId, Enum type, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            int length = payload.Length + 10;

            writer.Write(length);
            writer.Write(messageId);
            writer.Write(Convert.ToInt32(type));
            writer.Write(payload);
            writer.Write((short)0);

            await writer.BaseStream.FlushAsync();
        }

        private async Task HandleClient(ClientApi<TEnumRequest> clientApi)
        {
            try
            {
                while (clientApi.Client.Connected)
                {
                    clientApi.ClientData.Update(clientApi.BinaryReader);
                    if (RequestFuncs.TryGetValue(clientApi.ClientData.PacketTypeRequest, out Func<ClientApi<TEnumRequest>, Task> func))
                    {
                        await func(clientApi);
                    }
                    else
                    {
                        throw new NotFoundPacketTypeException(clientApi.ClientData.PacketTypeInt);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Exceptions.TryGetValue(ex.GetType(), out Func<ClientApi<TEnumRequest>, Exception, Task> func))
                {
                    await func(clientApi, ex);
                } else
                {
                    ServerError?.Invoke(ex);
                }
            }
            finally
            {
                ClientClose(clientApi);
            }
        }
    }
}
