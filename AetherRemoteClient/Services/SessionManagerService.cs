using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Components;
using AetherRemoteClient.Domain;
using AetherRemoteClient.UI.Windows;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services;

public class SessionManagerService
{
    // Injected
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;
    private readonly WindowSystem windowSystem;

    // Accessor
    private readonly GlamourerAccessor glamourerAccessor;

    // Providers
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
    private readonly SecretProvider secretProvider;

    private readonly Dictionary<int, ControlWindow> windows = new();

    public SessionManagerService(IPluginLog logger, ITargetManager targetManager, WindowSystem windowSystem,
        GlamourerAccessor glamourerAccessor, NetworkProvider networkProvider, EmoteProvider emoteProvider, SecretProvider secretProvider)
    {
        this.logger = logger;
        this.targetManager = targetManager;
        this.windowSystem = windowSystem;

        this.glamourerAccessor = glamourerAccessor;

        this.networkProvider = networkProvider;
        this.emoteProvider = emoteProvider;
        this.secretProvider = secretProvider;
    }

    public void StartSession(List<Friend> selectedFriends)
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

        var window = new ControlWindow(logger, targetManager, glamourerAccessor, networkProvider, emoteProvider, secretProvider, this, hash, selectedFriends, sb.ToString());
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
