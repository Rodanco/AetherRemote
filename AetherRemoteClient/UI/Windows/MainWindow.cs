using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.UI.Windows.Popups;
using AetherRemoteClient.UI.Windows.Views;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class MainWindow : Window
{
    // Configuration
    private readonly Configuration configuration;

    // Views
    private readonly DashboardView dashboardView;
    private readonly LoginView loginView;

    private IWindow currentView;

    private const ImGuiWindowFlags MainWindowWindowFlags =
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoResize;

    public MainWindow(Plugin plugin) : base("Aether Remote", MainWindowWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(275, 425),
            MaximumSize = new Vector2(275, 425)
        };

        // Configuration
        configuration = plugin.Configuration;

        // Views
        dashboardView = new DashboardView(plugin, this);
        loginView = new LoginView(plugin, this);

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
