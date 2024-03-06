using AetherRemoteClient.Domain;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class LogWindow : Window
{
    public LogWindow() : base("Aether Remote Logs")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 250),
            MaximumSize = new Vector2(400, 800)
        };
    }

    public override void Draw()
    {
        foreach (var log in ActionHistory.Logs)
        {
            ImGui.TextUnformatted(log.Message);
        }
    }
}
