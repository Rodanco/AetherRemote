using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs.Dashboard;
using AetherRemoteClient.UI.Experimental.Tabs.Friends;
using AetherRemoteClient.UI.Experimental.Tabs.Logs;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions;
using AetherRemoteClient.UI.Experimental.Tabs.Settings;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI;

public class MainWindow : Window
{
    // Constants
    private const ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.None;

    // Injected
    private readonly NetworkProvider networkProvider;

    // Tabs
    private readonly DashboardTab dashboardTab;
    private readonly FriendsTab friendsTab;
    private readonly SessionsTab sessionsTab;
    private readonly LogsTab logsTab;
    private readonly SettingsTab settingsTab;

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

        this.networkProvider = networkProvider;

        dashboardTab = new DashboardTab(configuration, friendListProvider, networkProvider, secretProvider);
        friendsTab = new FriendsTab(friendListProvider, networkProvider, secretProvider, logger);
        sessionsTab = new SessionsTab(glamourerAccessor, emoteProvider, friendListProvider, networkProvider, secretProvider, logger, targetManager);
        logsTab = new LogsTab();
        settingsTab = new SettingsTab(configuration);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            dashboardTab.Draw();
            if (networkProvider.ConnectionState == ServerConnectionState.Connected)
            {
                friendsTab.Draw();
                sessionsTab.Draw();
                logsTab.Draw();
            }
            settingsTab.Draw();

            ImGui.EndTabBar();
        }
    }
}
