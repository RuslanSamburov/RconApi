using Exiled.API.Features;
using RconApi.API.Features;
using RconAuth;
using RconAuth.Enums;
using RconScpSL.Configs;
using System;

namespace RconScpSL
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Руслан0308c";
        public override string Name => "Rcon";
        public override Version Version => new(1, 0, 0);
        public override Version RequiredExiledVersion => new(8, 14, 0);

        public static Plugin Singleton;

        private RconAuthServer _rconAuthServer;

        public override void OnEnabled()
        {
            Singleton = this;

            _rconAuthServer = new RconAuthServer(Config.RconPort, Config.IPAddress, Config.RconPassword);

            _rconAuthServer.Command += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                string response = Server.ExecuteCommand(clientApi.ClientData.Payload);
                RconAuthServer.SendResponse(clientApi.BinaryWriter, clientApi.ClientData.MessageId, PacketTypeResponse.ResponseValue, response);
            };

            _rconAuthServer.StartServer();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Singleton = null;

            _rconAuthServer.StopServer();

            base.OnDisabled();
        }
    }
}
