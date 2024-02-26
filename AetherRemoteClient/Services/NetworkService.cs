using AetherRemoteClient.Services.Network;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services;

public class NetworkService : IDisposable
{
    // Injected
    private readonly IPluginLog logger;
    private readonly SaveService saveService;

    // Network Variables
    private readonly HubConnection connection;
    private const string ConnectionUrl = "http://75.73.3.71:25565/mainHub";

    // Handlers
    public readonly NetworkCommandInvoker Commands;
    public readonly NetworkCommandHandler Handler;

    // Public
    public string? FriendCode = null;
    public HubConnectionState ConnectionStatus => connection.State;

    public NetworkService(Plugin plugin)
    {
        logger = plugin.Logger;
        saveService = plugin.SaveService;

        connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();

        Commands = new NetworkCommandInvoker(connection, this, saveService, logger);
        Handler = new NetworkCommandHandler(connection, plugin.ChatService, plugin.EmoteService, plugin.GlamourerAccessor, 
            logger, plugin.PluginInterface.Sanitizer, plugin.ClientState);
    }

    public async Task<bool> Connect()
    {
        if (Plugin.DeveloperMode) return true;
        if (connection.State != HubConnectionState.Disconnected) return false;

        logger.Info($"Attempting to connecting to server");

        try
        {
            await Task.Run(() => connection.StartAsync());

            if (connection.State == HubConnectionState.Connected)
            {
                logger.Info($"Successfully connected to server");
                return true;
            }
        }
        catch (HttpRequestException)
        {
            logger.Info($"Connection refused! The server is likely down");
        }
        catch (Exception ex)
        {
            logger.Info(ex.Message);
        }

        logger.Info($"Failed to connect to server");
        return false;
    }

    public async void Disconnect()
    {
        if (connection.State == HubConnectionState.Disconnected) return;
        await Task.Run(() => connection.StopAsync());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
