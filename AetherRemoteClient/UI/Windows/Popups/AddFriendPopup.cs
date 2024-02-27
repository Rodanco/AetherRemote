using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using ImGuiNET;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Windows.Popups;

public class AddFriendPopup : IPopupWindow
{
    private readonly FriendListService friendList;

    public string Name { get; set; } = "AddFriendPopup";

    public string AddFriendFriendId = string.Empty;
    public string AddFriendFriendNote = string.Empty;

    private bool attemptingToAddFriend = false;
    private AsyncResult? latestAddFriendResult = null;

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

            var isEmpty = AddFriendFriendId == string.Empty;
            if (isEmpty) ImGui.BeginDisabled();
            if (ImGui.Button("Add Friend", new Vector2(220, 28)))
            {
                var friendId = AddFriendFriendId;
                var friendNote = AddFriendFriendNote == string.Empty ? null : AddFriendFriendNote;
                var friendToAdd = new Friend(friendId, friendNote);

                Task.Run(() => TryAddFriend(friendToAdd));
            }
            if (isEmpty) ImGui.EndDisabled();

            if (latestAddFriendResult != null)
            {
                if (latestAddFriendResult.Success)
                {
                    AddFriendFriendId = string.Empty;
                    AddFriendFriendNote = string.Empty;
                    latestAddFriendResult = null;

                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    SharedUserInterfaces.TextCentered(latestAddFriendResult.Message + "longer longer");
                }
            }

            if (attemptingToAddFriend)
            {
                SharedUserInterfaces.TextCentered("Attempting to add friend...");
            }

            ImGui.EndPopup();
        }
    }

    private async void TryAddFriend(Friend friendToAdd)
    {
        attemptingToAddFriend = true;
        latestAddFriendResult = await friendList.AddFriend(friendToAdd);
        attemptingToAddFriend = false;
    }
}
