using RconApi.API;
using RconApi.API.EventsArgs;
using RconApi.API.Features;
using RconApi.API.Features.Clients;
using RconAuth.Enums;
using RconAuth.Exceptions;
using RconAuth.Features;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RconAuth
{
    public class RconAuthServer : Rcon
    {
        private readonly string RconPassword;

		public EventsAuth EventsAuth = new();

		public RconAuthServer(int port, IPAddress ipAddress, string rconPassword) : base(port, ipAddress)
        {
            RconPassword = rconPassword;

            Events.ClientConnected += (ClientConnectedEventArgs client) => client.Client.MetaData.SetMetadata("auth", false);

            _exceptions.Add(typeof(NotAuthException), async Task (Client client, Exception ex) =>
            {
                if (ex is NotAuthException auth)
                {
					EventsAuth.OnNotAuthenticated(client);
                    await client.SendResponse(auth.Message, (int)PacketTypeResponse.ResponseAuth, -1);
                    client.Disconnect();
                }
            });

            _requestFuncs.Add((int)PacketTypeRequest.Authentication, async Task (Client client) =>
            {
                if (RconPassword == client.Data.Payload)
                {
                    client.MetaData.SetMetadata("auth", true);
                    EventsAuth.OnAuthenticated(client);
                    await client.SendResponse("Authentication successful.", (int)PacketTypeResponse.ResponseAuth, client.Data.MessageId);
                }
                else
                {
                    throw new NotAuthException();
                }
            });

            _requestFuncs.Add((int)PacketTypeRequest.Command, async Task (Client client) =>
            {
                if (client.MetaData.GetMetadata<bool>("auth"))
                {
                    EventsAuth.OnCommand(client);
                }
                else
                {
                    throw new NotAuthException();
                }
            });
        }
    }
}
