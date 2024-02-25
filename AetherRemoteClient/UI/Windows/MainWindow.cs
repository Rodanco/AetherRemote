using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Views;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class MainWindow : Window
{
    // Modules
    private readonly NetworkService networkModule;

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
            MinimumSize = new Vector2(250, 400),
            MaximumSize = new Vector2(250, 400)
        };

        // Modules
        networkModule = plugin.NetworkService;

        // Views
        dashboardView = new DashboardView(plugin, this);
        loginView = new LoginView(plugin, this);

        // TODO: Automatic login
        currentView = loginView;
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
