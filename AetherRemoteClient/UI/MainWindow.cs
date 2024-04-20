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
        Configuration configuration,
        EmoteProvider emoteProvider,
        FriendListProvider friendListProvider,
        GlamourerAccessor glamourerAccessor,
        NetworkProvider networkProvider,
        SecretProvider secretProvider,
        IPluginLog logger,
        ITargetManager targetManager
        ) : base($"Aether Remote - Version {Plugin.Version}", MainWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = ImGui.GetIO().DisplaySize,
        };

        tabs =
        [
            new DashboardTab(configuration, friendListProvider, networkProvider, secretProvider),
            new FriendsTab(friendListProvider, networkProvider, secretProvider, logger),
            new SessionsTab(glamourerAccessor, emoteProvider, friendListProvider, networkProvider, secretProvider, logger, targetManager),
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
