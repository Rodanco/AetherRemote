using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs;
using AetherRemoteClient.UI.Experimental.Tabs.Dashboard;
using AetherRemoteClient.UI.Experimental.Tabs.Friends;
using AetherRemoteClient.UI.Experimental.Tabs.Logs;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions;
using AetherRemoteClient.UI.Experimental.Tabs.Settings;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Experimental;

public class MainWindowExperiment : Window
{
    private const ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.None;
    private readonly List<ITab> tabs;
    public MainWindowExperiment(
        NetworkProvider networkProvider, 
        FriendListProvider friendListProvider, 
        IPluginLog logger, 
        Configuration configuration, 
        SecretProvider secretProvider,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        ITargetManager targetManager
        ) : base("Aether Remote - Version 1.0.0.0", MainWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = ImGui.GetIO().DisplaySize,
        };

        tabs =
        [
            new DashboardTabExperimental(networkProvider),
            new FriendsTabExperimental(friendListProvider, networkProvider, secretProvider, logger),
            new SessionsTabExperimental(friendListProvider, emoteProvider, glamourerAccessor, logger, targetManager),
            new LogsTabExperimental(),
            new SettingsTabExperimental(configuration),
        ];
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            foreach (var tab in tabs)
            {
                tab.Draw();
            }

            ImGui.EndTabBar();
        }
    }
}
