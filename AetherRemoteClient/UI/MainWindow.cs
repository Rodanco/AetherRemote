using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs.Dashboard;
using AetherRemoteClient.UI.Experimental.Tabs.Friends;
using AetherRemoteClient.UI.Experimental.Tabs.Logs;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions;
using AetherRemoteClient.UI.Experimental.Tabs.Settings;
using AetherRemoteClient.UI.Tabs;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI;

public class MainWindow : Window
{
    private const ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.None;
    private readonly List<ITab> tabs;

    public MainWindow(
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
            new DashboardTab(networkProvider),
            new FriendsTab(friendListProvider, networkProvider, secretProvider, logger),
            new SessionsTab(friendListProvider, secretProvider, networkProvider, emoteProvider, glamourerAccessor, logger, targetManager),
            new LogsTab(),
            new SettingsTab(configuration)
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
