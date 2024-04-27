using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs.Dashboard;
using AetherRemoteClient.UI.Experimental.Tabs.Friends;
using AetherRemoteClient.UI.Experimental.Tabs.Logs;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions;
using AetherRemoteClient.UI.Experimental.Tabs.Settings;
using AetherRemoteClient.UI.Tabs.Control;
using AetherRemoteClient.UI.Tabs.MassControl;
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

    // Statics
    public static readonly Vector2 FriendListSize = new(150, 0);

    // Injected
    private readonly NetworkProvider networkProvider;

    // Tabs
    private readonly DashboardTab dashboardTab;
    private readonly FriendsTab friendsTab;
    private readonly SessionsTab sessionsTab;
    private readonly LogsTab logsTab;
    private readonly SettingsTab settingsTab;
    private readonly ControlTab controlTab;
    private readonly MassControlTab massControlTab;

    public MainWindow(
        Configuration configuration,
        EmoteProvider emoteProvider,
        FriendListProvider friendListProvider,
        GlamourerAccessor glamourerAccessor,
        NetworkProvider networkProvider,
        SecretProvider secretProvider,
        IClientState clientState,
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
        controlTab = new ControlTab(glamourerAccessor, emoteProvider, friendListProvider, networkProvider, secretProvider, clientState, logger, targetManager);
        massControlTab = new MassControlTab();
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            dashboardTab.Draw();
            if (Plugin.DeveloperMode || networkProvider.ConnectionState == ServerConnectionState.Connected)
            {
                friendsTab.Draw();
                controlTab.Draw();
                massControlTab.Draw();
                sessionsTab.Draw();
                logsTab.Draw();
            }
            settingsTab.Draw();

            ImGui.EndTabBar();
        }
    }
}
