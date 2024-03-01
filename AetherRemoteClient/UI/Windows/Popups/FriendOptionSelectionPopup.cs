using AetherRemoteClient.Domain.Interfaces;
using ImGuiNET;

namespace AetherRemoteClient.UI.Windows.Popups;

public class FriendOptionSelectionPopup : IPopupWindow
{
    public string Name { get; set; } = "FriendOptionSelectionPopup";

    public void Draw()
    {
        if (ImGui.BeginPopup(Name))
        {
            SharedUserInterfaces.Icon(Dalamud.Interface.FontAwesomeIcon.User);
            ImGui.SameLine();
            ImGui.MenuItem("Edit");

            if (ImGui.BeginMenu("Delete"))
            {
                ImGui.MenuItem("Confirm");
                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }
}
