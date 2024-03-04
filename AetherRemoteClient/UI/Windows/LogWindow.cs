using AetherRemoteClient.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class LogWindow : Window
{
    private readonly LogService logService;

    public LogWindow(LogService logService) : base("Aether Remote Logs")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 250),
            MaximumSize = new Vector2(400, 800)
        };

        this.logService = logService;
    }

    public override void Draw()
    {
        foreach (var log in logService.Logs)
        {
            ImGui.TextUnformatted(log.Message);
        }
    }
}
