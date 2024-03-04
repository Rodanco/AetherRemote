using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Popups;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows.Views;

public class DashboardView : IWindow
{
    // Inject
    private readonly UiBuilder uiBuilder;
    private readonly ConfigWindow configWindow;
    private readonly Configuration configuration;
    private readonly FriendListService friendList;
    private readonly NetworkService networkService;
    private readonly MainWindow mainWindow;
    private readonly IPluginLog logger;

    // Filter
    //private readonly FastFilter<Friend> friendsListFilter;

    // Variable used to search friends list
    private string searchByFriendIdOrNoteInput = string.Empty;

    // Popup Windows
    private readonly AddFriendPopup addFriendPopup;
    private readonly EditFriendPopup editFriendPopup;
    private readonly FriendOptionSelectionPopup friendOptionSelectionPopup;

    private const ImGuiTreeNodeFlags TreeNodeFlags =
            ImGuiTreeNodeFlags.SpanFullWidth |
            ImGuiTreeNodeFlags.DefaultOpen |
            ImGuiTreeNodeFlags.FramePadding;

    CustomFilter<Friend> friendFilter;

    public DashboardView(Plugin plugin, MainWindow mainWindow)
    {
        // Inject
        configWindow = plugin.ConfigWindow;
        uiBuilder = plugin.PluginInterface.UiBuilder;
        friendList = plugin.FriendListService;
        networkService = plugin.NetworkService;
        configuration = plugin.Configuration;
        logger = plugin.Logger;
        this.mainWindow = mainWindow;

        //friendsListFilter = new FastFilter<Friend>(friendList.Friends);

        // Define Popups
        addFriendPopup = new AddFriendPopup(friendList);
        editFriendPopup = new EditFriendPopup(friendList);
        friendOptionSelectionPopup = new FriendOptionSelectionPopup();

        friendFilter = new CustomFilter<Friend>(friendList.Friends, (friend, search) => friend.NoteOrId.Contains(search, System.StringComparison.OrdinalIgnoreCase));
    }

    public void Draw()
    {
        if (!Plugin.DeveloperMode && networkService.ConnectionStatus == HubConnectionState.Disconnected)
        {
            mainWindow.SetCurrentViewToLogin();
            return;
        }

        var padding = ImGui.GetStyle().FramePadding;

        var friendCode = networkService.FriendCode ?? "Unknown";

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Copy, 1.5f))
        {
            ImGui.SetClipboardText(friendCode);
            uiBuilder.AddNotification("Successfully copied id to clipboard", "Aether Remote", NotificationType.Success);
        }

        ImGui.SameLine();

        // 35 is the size of the buttons, 8 is the default padding. Yay magic numbers!
        var workingSpace = ImGui.GetWindowWidth() - ((2 * 35) - (4 * 8));
        SharedUserInterfaces.DynamicTextCentered(friendCode, workingSpace, SharedUserInterfaces.Gold);

        var configWindowButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.Wrench, 1.5f);
        ImGui.SameLine(ImGui.GetWindowWidth() - configWindowButtonSize.X - 8);
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Wrench, 1.5f))
        {
            configWindow.IsOpen = true;
        }

        ImGui.Separator();
        ImGui.Spacing();

        var addFriendButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.UserPlus);
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - addFriendButtonSize.X - (padding.X * 6));

        if (ImGui.InputTextWithHint("###SearchFriend", "Search Friend", ref searchByFriendIdOrNoteInput,
            AetherRemoteConstants.FriendCodeCharLimit))
            friendFilter.Restart(searchByFriendIdOrNoteInput);
        

        ImGui.SameLine();
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.UserPlus, addFriendButtonSize))
        {
            ImGui.OpenPopup(friendOptionSelectionPopup.Name);
        }

        friendOptionSelectionPopup.Draw();

        //var filteredList = friendsListFilter.Filter(searchByFriendIdOrNoteInput);
        IList<Friend> listToUse = friendFilter.List;
        //if (useFilterList)
        //    listToUse = filteredFriends;
        //else
        //    listToUse = friendList.Friends;
        var filteredOnlineList = new List<Friend>();
        var filteredOfflineList = new List<Friend>();
        foreach (var friend in listToUse)
        {
            if (friend.Online)
                filteredOnlineList.Add(friend);
            else
                filteredOfflineList.Add(friend);
        }

        var friendListAreaSize = ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 50;
        ImGui.BeginChild("FriendListArea", new Vector2(0, friendListAreaSize), false, ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (ImGui.TreeNodeEx("Online", TreeNodeFlags))
        {
            foreach (var friend in filteredOnlineList)
            {
                var userSettingButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.UserCog, 1.0f);
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.1f, 0.5f));

                var selectableId = $"{friend.NoteOrId}###{friend.FriendCode}";
                //var selectableSize = new Vector2(ImGui.GetWindowWidth() - (padding.X * 8) - userSettingButtonSize.X, userSettingButtonSize.Y);
                var selectableSize = new Vector2(0, userSettingButtonSize.Y);

                if (ImGui.Selectable(selectableId, friend.Selected, ImGuiSelectableFlags.SpanAllColumns, selectableSize))
                {
                    friend.Selected = !friend.Selected;
                }

                ImGui.SetItemAllowOverlap();

                ImGui.PopStyleVar();

                ImGui.SameLine();
                ImGui.SetCursorPosX(8);
                SharedUserInterfaces.Icon(FontAwesomeIcon.User, SharedUserInterfaces.Green);

                //ImGui.SameLine();
                //SharedUserInterfaces.ColorText(friend.NoteOrId, SharedUserInterfaces.White);

                ImGui.SameLine(ImGui.GetWindowWidth() - 22 - 22);
                if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.UserCog, userSettingButtonSize, $"{friend.FriendCode}-SettingsButton"))
                {
                    editFriendPopup.Open(friend);
                }

            }

            editFriendPopup.Draw();

            ImGui.TreePop();
        }


        if (ImGui.TreeNodeEx("Offline", TreeNodeFlags))
        {
            foreach (var friend in filteredOfflineList)
            {
                ImGui.SetCursorPosX(15);
                SharedUserInterfaces.Icon(FontAwesomeIcon.User, SharedUserInterfaces.Red);

                ImGui.SameLine();
                SharedUserInterfaces.ColorText(friend.NoteOrId, SharedUserInterfaces.Grey);

                ImGui.SameLine(ImGui.GetWindowWidth() - 22 - 22);
                if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.UserCog, 1.0f, $"{friend.FriendCode}-SettingsButton"))
                {
                    editFriendPopup.Open(friend);
                }
            }

            editFriendPopup.Draw();

            ImGui.TreePop();
        }

        ImGui.EndChild();

        var friendListCount = friendList.SelectedFriends.Count <= 0;
        if (friendListCount) ImGui.BeginDisabled();
        if (ImGui.Button("Control", new Vector2(ImGui.GetWindowWidth() - 15, 40)))
        {
            networkService.StartSession(friendList.SelectedFriends);
        }
        if (friendListCount) ImGui.EndDisabled();
    }
}
