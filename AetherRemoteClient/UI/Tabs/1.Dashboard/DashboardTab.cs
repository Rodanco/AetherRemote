using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Experimental.Tabs.Dashboard;

public class DashboardTab(NetworkProvider networkProvider) : ITab
{
    public readonly NetworkProvider networkProvider = networkProvider;

    public void Draw()
    {
        if (ImGui.BeginTabItem("Dashboard"))
        {
            if (ImGui.BeginChild("DashboardArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.BigTextCentered("Aether Remote", ImGuiColors.ParsedOrange);
                SharedUserInterfaces.MediumTextCentered("Version 1.0.0.0");
                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }
}
