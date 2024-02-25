using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using Dalamud.Logging;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows.Popups;

public class AddFriendPopup : IPopupWindow
{
    private readonly FriendListService friendList;

    public string Name { get; set; } = "AddFriendPopup";

    public string AddFriendFriendId = string.Empty;
    public string AddFriendFriendNote = string.Empty;

    public AddFriendPopup(FriendListService friendList)
    {
        this.friendList = friendList;
    }

    public void Draw()
    {
        if (ImGui.BeginPopup(Name, SharedUserInterfaces.PopupWindowFlags))
        {
            SharedUserInterfaces.MediumText("Add Friend", SharedUserInterfaces.Gold);
            ImGui.Separator();
            ImGui.Text("Friend Id");
            ImGui.SetNextItemWidth(220);
            ImGui.InputText("###Friend Id", ref AddFriendFriendId, 
                AetherRemoteConstants.FriendCodeCharLimit);
            ImGui.Text("Nickname");
            ImGui.SetNextItemWidth(220);
            ImGui.InputTextWithHint("###Nickname", "(Optional)", ref AddFriendFriendNote, 
                AetherRemoteConstants.FriendNicknameCharLimit);
            ImGui.Spacing();
            ImGui.Spacing();
            if (AddFriendFriendId == string.Empty) ImGui.BeginDisabled();
            if (ImGui.Button("Add Friend", new Vector2(220, 28)))
            {
                var friendId = AddFriendFriendId;
                var friendNote = AddFriendFriendNote == string.Empty ? null : AddFriendFriendNote;
                var friendToAdd = new Friend(friendId, friendNote);
                PluginLog.Information($"{friendToAdd}");
                friendList.AddFriend(friendToAdd);

                AddFriendFriendId = string.Empty;
                AddFriendFriendNote = string.Empty;

                // TODO: Solve the mystery of why this is needed.
                // Fixes a bug where adding a new friend while
                // having friends currently selected causes the
                // entire UI to become disabled. My guess is something to do
                // with the pop up ending 'early' and somehow ImGui.BeginDisabled is
                // called without ever hitting the ImGui.EndDisabled
                ImGui.EndDisabled();

                ImGui.CloseCurrentPopup();
            }
            if (AddFriendFriendId == string.Empty) ImGui.EndDisabled();

            ImGui.EndPopup();
        }


    }
}
