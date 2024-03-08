using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows.Popups;

public class EditFriendPopup : IPopupWindow
{
    public string Name { get; set; } = "EditFriendPopup";

    private static readonly Vector2 ButtonSize = new(220, 28);

    private readonly FriendListService friendList;
    private Friend? friendWhoIsBeingEdited = null;
    private Friend? friendWhoIsBeingEditedTemp = null;

    /// <summary>
    /// Is the popup window currently open.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    public EditFriendPopup(FriendListService friendList)
    {
        this.friendList = friendList;
    }

    [Obsolete("Please use Open instead.")]
    public void Edit(Friend friendToEdit)
    {
        friendWhoIsBeingEdited = friendToEdit;
        friendWhoIsBeingEditedTemp = friendToEdit.Copy();
    }

    public void Open(Friend friendToEdit)
    {
        friendWhoIsBeingEdited = friendToEdit;
        friendWhoIsBeingEditedTemp = friendToEdit.Copy();

        var _x = ImGui.GetWindowPos().X + ImGui.GetWindowWidth() + 8;
        var _y = ImGui.GetMousePos().Y - 50;

        ImGui.SetNextWindowPos(new Vector2(_x, _y));
        ImGui.OpenPopup(Name);
    }

    public void Draw()
    {
        if (ImGui.BeginPopup(Name))
        {
            IsActive = true;

            if (friendWhoIsBeingEdited == null || friendWhoIsBeingEditedTemp == null)
                return;

            var shouldResetFriendSelect = false;
            var id = friendWhoIsBeingEditedTemp.FriendCode;
            var note = friendWhoIsBeingEditedTemp.Note ?? string.Empty;

            SharedUserInterfaces.MediumText("Settings", ImGuiColors.ParsedOrange);
            ImGui.Separator();
            ImGui.Text("Id");
            ImGui.SetNextItemWidth(220);
            ImGui.InputText("###Friend Id", ref id, 
                AetherRemoteConstants.FriendCodeCharLimit);
            ImGui.Text("Nickname");
            ImGui.SetNextItemWidth(220);
            ImGui.InputTextWithHint("###Nickname", "(Optional)", ref note, 
                AetherRemoteConstants.FriendNicknameCharLimit);

            ImGui.Spacing();
            ImGui.Spacing();
            if (ImGui.Button("Save", ButtonSize))
            {
                friendList.UpdateFriend(friendWhoIsBeingEdited, friendWhoIsBeingEditedTemp);
                shouldResetFriendSelect = true;

                ImGui.CloseCurrentPopup();
            }

            ImGui.Spacing();
            if (ImGui.Button("Delete", ButtonSize))
            {
                friendList.RemoveFriend(friendWhoIsBeingEdited);
                shouldResetFriendSelect = true;

                ImGui.CloseCurrentPopup();
            }

            if (shouldResetFriendSelect)
            {
                friendWhoIsBeingEdited = null;
                friendWhoIsBeingEditedTemp = null;
            }
            else
            {
                friendWhoIsBeingEditedTemp.FriendCode = id;
                friendWhoIsBeingEditedTemp.Note = note == string.Empty ? null : note;
            }

            ImGui.EndPopup();
        }
        else
        {
            IsActive = false;
        }
    }
}
