using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services.Network;
using AetherRemoteClient.UI.Windows;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services;

public class NetworkService : IDisposable
{
    // Injected
    private readonly IPluginLog logger;
    private readonly SaveService saveService;

    // Instantiated
    private readonly SessionManager sessionManager;

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

        sessionManager = new(logger, plugin.TargetManager, this, plugin.WindowSystem, plugin.EmoteService, plugin.GlamourerAccessor);

        connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();

        Commands = new NetworkCommandInvoker(connection, this, saveService, logger);
        Handler = new NetworkCommandHandler(connection, plugin.ChatService, plugin.EmoteService, plugin.GlamourerAccessor, 
            logger, plugin.PluginInterface.Sanitizer, plugin.ClientState);

        connection.Closed += ConnectionClosed;
    }

    public void StartSession(List<Friend> selectedFriends)
    {
        sessionManager.MakeSession(selectedFriends);
    }

    public void EndSession(int hash)
    {
        sessionManager.EndSession(hash);
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
        if (connection.State != HubConnectionState.Disconnected)
            Disconnect();

        connection.Closed -= ConnectionClosed;
        GC.SuppressFinalize(this);
    }

    private Task ConnectionClosed(Exception? exception)
    {
        if (exception != null)
            logger.Info($"Connection closed with exception: {exception.Message}");

        sessionManager.EndAllSessions();
        return Task.CompletedTask;
    }

    private class SessionManager
    {
        // Injected
        private readonly WindowSystem windowSystem;
        private readonly IPluginLog logger;
        private readonly ITargetManager targetManager;
        private readonly EmoteService emoteService;
        private readonly NetworkService networkService;
        private readonly GlamourerAccessor glamourerAccessor;

        private readonly Dictionary<int, ControlWindow> windows = new();

        public SessionManager(IPluginLog logger, ITargetManager targetManager, NetworkService networkService,
            WindowSystem windowSystem, EmoteService emoteService, GlamourerAccessor glamourerAccessor)
        {
            this.windowSystem = windowSystem;
            this.logger = logger;
            this.targetManager = targetManager;
            this.emoteService = emoteService;
            this.networkService = networkService;
            this.glamourerAccessor = glamourerAccessor;
        }

        public void MakeSession(List<Friend> selectedFriends)
        {
            if (selectedFriends.Count == 0) return;

            var sb = new StringBuilder();
            var hash = ComputeHash(selectedFriends);
            if (windows.ContainsKey(hash))
            {
                logger.Info("Already controlling these people");
                return;
            }

            sb.Append(selectedFriends.OrderBy(friend => friend.NoteOrId).First().NoteOrId);
            if (selectedFriends.Count > 1)
            {
                sb.Append(" and ");
                sb.Append(selectedFriends.Count - 1);
                sb.Append(" other");
                if (selectedFriends.Count > 2)
                    sb.Append('s');
            }
            sb.Append(" [");
            sb.Append(hash.ToString("X8"));
            sb.Append(']');

            var window = new ControlWindow(sb.ToString(), hash, selectedFriends, logger, targetManager, emoteService, networkService, glamourerAccessor);
            windowSystem.AddWindow(window);
            window.IsOpen = true;

            windows.Add(hash, window);
        }

        public void EndSession(int hash)
        {
            if (windows.TryGetValue(hash, out var window))
            {
                if (window == null) 
                    return;

                RemoveWindow(hash, window);
            }
        }

        public void EndAllSessions()
        {
            foreach (var kvp in windows)
            {
                var hash = kvp.Key;
                var window = kvp.Value;

                RemoveWindow(hash, window);
            }
        }

        private void RemoveWindow(int hash, Window window)
        {
            window.IsOpen = false;
            windowSystem.RemoveWindow(window);
            windows.Remove(hash);
        }

        private static int ComputeHash(List<Friend> selectedFriends)
        {
            return string.Join("", selectedFriends.OrderBy(friend => friend.FriendCode)).GetHashCode();
        }
    }
}
