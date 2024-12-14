using RconApi.API;
using RconApi.API.Abstracts;
using RconApi.API.EventsArgs;
using RconApi.API.Features;
using RconAuth.Enums;
using RconAuth.Exceptions;
using RconAuth.Features;
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
        private readonly string RconPassword;
        private readonly ConcurrentDictionary<TcpClient, bool> ClientStates = new();

		public EventsAuth<PacketTypeRequest> EventsAuth = new();

		public RconAuthServer(int port, IPAddress ipAddress, string rconPassword) : base(port, ipAddress)
        {
            RconPassword = rconPassword;

            Events.ClientConnected += (ClientConnectedEventArgs client) => ClientStates[client.TcpClient] = false;
            Events.ClientDisconnecting += (ClientDisconnectingEventArgs client) => ClientStates.TryRemove(client.TcpClient, out _);

			Events.ServerStopping += () =>
            {
                foreach (KeyValuePair<TcpClient, bool> client in ClientStates)
                {
                    AClient clientApi = new(client.Key);

                    ClientClose(clientApi.TcpClient);
                }
            };

            Exceptions.Add(typeof(NotAuthException), async Task (ClientApi<PacketTypeRequest> clientApi, Exception ex) =>
            {
                if (ex is NotAuthException auth)
                {
					EventsAuth.OnNotAuthenticated(clientApi);
                    await SendResponse(clientApi.BinaryWriter, -1, PacketTypeResponse.ResponseAuth, auth.Message);
                    ClientClose(clientApi.TcpClient);
                }
            });

            RequestFuncs.Add(PacketTypeRequest.Authentication, async Task (ClientApi<PacketTypeRequest> clientApi) =>
            {
                if (RconPassword == clientApi.ClientData.Payload)
                {
                    ClientStates[clientApi.TcpClient] = true;
                    EventsAuth.OnAuthenticated(clientApi);
                    await SendResponse(clientApi.BinaryWriter, clientApi.ClientData.MessageId, PacketTypeResponse.ResponseAuth, "Authentication successful.");
                }
                else
                {
                    throw new NotAuthException();
                }
            });

            RequestFuncs.Add(PacketTypeRequest.Command, async Task (ClientApi<PacketTypeRequest> clientApi) =>
            {
                if (ClientStates[clientApi.TcpClient])
                {
                    EventsAuth.OnCommand(clientApi);
                }
                else
                {
                    throw new NotAuthException();
                }
            });
        }
    }
}
