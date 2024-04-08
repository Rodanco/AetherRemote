using AetherRemoteClient.Domain;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Experimental.Tabs.Logs;

public class LogsTabExperimental : ITab
{
    private readonly ThreadedFilter<LogEntry> threadedFilter = new(AetherRemoteLogging.Logs, FilterLogEntry);

    private string searchLogTerm = string.Empty;

    public void Draw()
    {
        if (ImGui.BeginTabItem("Logs"))
        {
            if (ImGui.InputTextWithHint("##SearchLog", "Search", ref searchLogTerm, 128))
            {
                threadedFilter.Restart(searchLogTerm);
            }

            ImGui.SameLine();
            SharedUserInterfaces.IconButton(FontAwesomeIcon.TrashAlt);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Clears all of the current logs");
                ImGui.EndTooltip();
            }

            if (ImGui.BeginChild("LogArea", Vector2.Zero, true))
            {
                for (var i = threadedFilter.List.Count - 1; i > 0; i--)
                {
                    var log = threadedFilter.List[i];
                    switch (log.Type)
                    {
                        case LogType.Sent:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.HealerGreen, "Sent");
                            break;

                        case LogType.Recieved:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.TankBlue, "Recieved");
                            break;

                        case LogType.Info:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextUnformatted("Info");
                            break;

                        case LogType.Error:
                            ImGui.TextUnformatted($"[{log.Timestamp.ToShortTimeString()}]");
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudRed, "Error");
                            break;
                    }

                    ImGui.TextUnformatted(log.Message);
                    ImGui.Separator();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    private static bool FilterLogEntry(LogEntry entry, string searchTerm)
    {
        // In the message
        if (entry.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        // In the sender
        if (entry.Sender.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
