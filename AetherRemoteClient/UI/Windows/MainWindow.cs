using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Views;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class MainWindow : Window
{
    // Views
    private readonly DashboardView dashboardView;
    private readonly LoginView loginView;

    private IWindow currentView;

    private const ImGuiWindowFlags MainWindowWindowFlags =
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoResize;

    public MainWindow(
        IPluginLog logger,
        DalamudPluginInterface pluginInterface,
        ConfigWindow configWindow,
        LogWindow logWindow,
        Configuration configuration,
        NetworkProvider networkProvider,
        SecretProvider secretProvider,
        FriendListService friendListService,
        SessionManagerService sessionManagerService
        ) : base("Aether Remote", MainWindowWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(275, 425),
            MaximumSize = new Vector2(275, 425)
        };

        TitleBarButtons = new()
        {
            new TitleBarButton()
            {
                Icon = FontAwesomeIcon.Cog,
                Click = (_) =>
                {
                    configWindow.IsOpen = !configWindow.IsOpen;
                },
                IconOffset = new Vector2(2, 1),
                ShowTooltip = () =>
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Open Settings");
                    ImGui.EndTooltip();
                }
            }
        };

        // Views
        dashboardView = new DashboardView(logger, pluginInterface, this, configWindow, logWindow, configuration, networkProvider, friendListService, sessionManagerService);
        loginView = new LoginView(logger, this, configuration, networkProvider, secretProvider, friendListService);
        
        currentView = loginView;

        if (configuration.AutoConnect)
            loginView.QueueLogin();
    }

    public override void Draw()
    {
        currentView.Draw();
    }

    public void SetCurrentViewToLogin()
    {
        currentView = loginView;
    }

    public void SetCurrentViewToDashboard()
    {
        currentView = dashboardView;
    }
}
