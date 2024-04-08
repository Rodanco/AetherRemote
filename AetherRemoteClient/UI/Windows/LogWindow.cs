using AetherRemoteClient.Domain;
using Dalamud.Interface.Colors;
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
            MaximumSize = new Vector2(800, 800)
        };
    }

    public override void Draw()
    {
        foreach (var log in AetherRemoteLogging.Logs)
        {
            SharedUserInterfaces.ColorText($"[{log.Type}] {log.Timestamp}", MessageColorMap(log.Type));
            ImGui.TextWrapped(log.Message);
            if (Plugin.DeveloperMode)
            {
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "(DeveloperMode)");
            }
            ImGui.Separator();
        }
    }

    private static Vector4 MessageColorMap(LogType type)
    {
        return type switch
        {
            LogType.Recieved => ImGuiColors.TankBlue,
            LogType.Sent => ImGuiColors.DalamudOrange,
            LogType.Error => ImGuiColors.DalamudRed,
            _ => ImGuiColors.DalamudGrey
        };
    }
}
