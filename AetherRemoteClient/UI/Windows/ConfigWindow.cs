using AetherRemoteClient.Services;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class ConfigWindow : Window
{
    private readonly NetworkService networkModule;

    public ConfigWindow(Plugin plugin) : base(
        "Aether Remote Config",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(300, 300);
        SizeCondition = ImGuiCond.Always;

        networkModule = plugin.NetworkService;
    }

    public override void Draw()
    {
        
    }
}
