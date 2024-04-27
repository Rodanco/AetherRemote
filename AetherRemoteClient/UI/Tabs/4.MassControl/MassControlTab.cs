using ImGuiNET;

namespace AetherRemoteClient.UI.Tabs.MassControl;

public class MassControlTab : ITab
{
    public void Draw()
    {
        if (ImGui.BeginTabItem("Mass Control"))
        {
            ImGui.EndTabItem();
        }
    }
}
