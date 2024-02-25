using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.UI.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AetherRemoteClient.Services;

public class SessionService
{
    private readonly Dictionary<int, Window> windows = new();
    private readonly WindowSystem windowSystem;
    private readonly IPluginLog logger;
    private readonly EmoteService emoteService;
    private readonly NetworkService networkService;
    private readonly GlamourerAccessor glamourerAccessor;

    public SessionService(Plugin plugin)
    {
        logger = plugin.Logger;
        windowSystem = plugin.WindowSystem;
        emoteService = plugin.EmoteService;
        networkService = plugin.NetworkService;
        glamourerAccessor = plugin.GlamourerAccessor;
    }

    public void MakeWindow(List<Friend> selectedFriends)
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

        var window = new ControlWindow(sb.ToString(), hash, selectedFriends, this, logger, emoteService, networkService, glamourerAccessor);
        windowSystem.AddWindow(window);
        window.IsOpen = true;

        windows.Add(hash, window);
    }

    public void RemoveWindow(Window window, int hash)
    {
        windows.Remove(hash);
        windowSystem.RemoveWindow(window);
    }

    public void RemoveAllWindows()
    {
        foreach (var kvp in windows)
        {
            var hash = kvp.Key;
            var window = kvp.Value;

            windowSystem.RemoveWindow(window);
            windows.Remove(hash);
        }
    }

    private static int ComputeHash(List<Friend> selectedFriends)
    {
        return string.Join("", selectedFriends.OrderBy(friend => friend.FriendCode)).GetHashCode();
    }
}
