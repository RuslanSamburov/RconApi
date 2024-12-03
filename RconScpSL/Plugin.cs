using Exiled.API.Features;
using RconApi.API.Features;
using RconAuth;
using RconAuth.Enums;
using RconScpSL.Configs;
using System;
using System.Net;

namespace RconScpSL
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Руслан0308c";
        public override string Name => "Rcon";
        public override Version Version => new(1, 0, 1);
        public override Version RequiredExiledVersion => new(8, 14, 0);

        public static Plugin Singleton;

        private RconAuthServer _rconAuthServer;

        public override void OnEnabled()
        {
            Singleton = this;

            _rconAuthServer = new (Config.RconPort, Config.IPAddress ? IPAddress.Any : IPAddress.Loopback, Config.RconPassword);

            _rconAuthServer.ServerStarting += () =>
            {
                Log.Info("Server staring...");
            };

            _rconAuthServer.ServerStarted += () =>
            {
                Log.Info("The server started on the port " + _rconAuthServer.Port);
            };

            _rconAuthServer.ClientConnected += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                IPEndPoint ep = clientApi.Client.Client.RemoteEndPoint as IPEndPoint;
                Log.Info($"lient with ip: {ep.Address} connected!");
            };

            _rconAuthServer.ClientDisconnecting += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                IPEndPoint ep = clientApi.Client.Client.RemoteEndPoint as IPEndPoint;
                Log.Info($"Client with ip: {ep.Address} disconnected!");
            };

            _rconAuthServer.Authenticated += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                IPEndPoint ep = clientApi.Client.Client.RemoteEndPoint as IPEndPoint;
                Log.Info($"Client with ip: {ep.Address} logged in!");
            };

            _rconAuthServer.NotAuthenticated += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                IPEndPoint ep = clientApi.Client.Client.RemoteEndPoint as IPEndPoint;
                Log.Error($"Client with ip: {ep.Address} did not log in!");
            };

            _rconAuthServer.Command += (ClientApi<PacketTypeRequest> clientApi) =>
            {
                IPEndPoint ep = clientApi.Client.Client.RemoteEndPoint as IPEndPoint;

                string response = Server.ExecuteCommand(clientApi.ClientData.Payload);
                RconAuthServer.SendResponse(clientApi.BinaryWriter, clientApi.ClientData.MessageId, PacketTypeResponse.ResponseValue, response);

                Log.Info($"Client with ip: {ep.Address} entered the command: {clientApi.ClientData.Payload}");
            };

            _rconAuthServer.ServerError += (Exception ex) =>
            {
                Log.Error("Server error: " + ex.Message);
            };

            _rconAuthServer.ServerStoping += () =>
            {
                Log.Info("Server stoping...");
            };

            _rconAuthServer.ServerStoped += () =>
            {
                Log.Info("Server stoping...");
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
