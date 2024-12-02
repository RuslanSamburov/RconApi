using RconApi.API;
using RconApi.API.Features;
using RconAuth.Enums;
using RconAuth.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RconAuth
{
    public class RconAuthServer : Rcon<PacketTypeResponse, PacketTypeRequest>
    {
        private string RconPassword;
        private readonly ConcurrentDictionary<TcpClient, bool> ClientStates = new();

        public event Action<ClientApi<PacketTypeRequest>> Authenticated;
        public event Action<ClientApi<PacketTypeRequest>> NotAuthenticated;

        public event Action<ClientApi<PacketTypeRequest>> Command;

        public RconAuthServer(int port, IPAddress ipAddress, string rconPassword) : base(port, ipAddress)
        {
            RconPassword = rconPassword;
            ClientConnected += (ClientApi<PacketTypeRequest> clientApi) => ClientStates[clientApi.Client] = false;
            ClientDisconnecting += (ClientApi<PacketTypeRequest> clientApi) => ClientStates.TryRemove(clientApi.Client, out _);

            ServerStoping += () =>
            {
                foreach (KeyValuePair<TcpClient, bool> client in ClientStates)
                {
                    ClientApi<PacketTypeRequest> clientApi = new(client.Key);

                    ClientClose(clientApi);
                }
            };

            Exceptions.Add(typeof(NotAuthException), async Task (ClientApi<PacketTypeRequest> clientApi, Exception ex) =>
            {
                if (ex is NotAuthException auth)
                {
                    NotAuthenticated?.Invoke(clientApi);
                    await SendResponse(clientApi.BinaryWriter, -1, PacketTypeResponse.ResponseAuth, auth.Message);
                    clientApi.Client.Close();
                }
            });

            RequestFuncs.Add(PacketTypeRequest.Authentication, async Task (ClientApi<PacketTypeRequest> clientApi) =>
            {
                if (RconPassword == clientApi.ClientData.Payload)
                {
                    ClientStates[clientApi.Client] = true;
                    Authenticated?.Invoke(clientApi);
                    await SendResponse(clientApi.BinaryWriter, clientApi.ClientData.MessageId, PacketTypeResponse.ResponseAuth, "Authentication successful.");
                }
                else
                {
                    throw new NotAuthException();
                }
            });

            RequestFuncs.Add(PacketTypeRequest.Command, async Task (ClientApi<PacketTypeRequest> clientApi) =>
            {
                if (ClientStates[clientApi.Client])
                {
                    Command?.Invoke(clientApi);
                }
                else
                {
                    throw new NotAuthException();
                }
            });
        }
    }
}
