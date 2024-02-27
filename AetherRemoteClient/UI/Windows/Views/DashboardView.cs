using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Popups;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
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
    private readonly FastFilter<Friend> friendsListFilter;

    // Variable used to search friends list
    private string searchByFriendIdOrNoteInput = string.Empty;

    // Popup Windows
    private readonly AddFriendPopup addFriendPopup;
    private readonly EditFriendPopup editFriendPopup;

    private const ImGuiTreeNodeFlags TreeNodeFlags =
            ImGuiTreeNodeFlags.SpanFullWidth |
            ImGuiTreeNodeFlags.DefaultOpen |
            ImGuiTreeNodeFlags.FramePadding;

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

        friendsListFilter = new FastFilter<Friend>(friendList.Friends);

        // Define Popups
        addFriendPopup = new AddFriendPopup(friendList);
        editFriendPopup = new EditFriendPopup(friendList);
    }

    public void Draw()
    {
        if (networkService.ConnectionStatus == HubConnectionState.Disconnected)
        {
            mainWindow.SetCurrentViewToLogin();
            return;
        }

        var friendCode = networkService.FriendCode;

        if (SharedUserInterfaces.IconButton(IconButtonArgs.CopyClipboard))
        {
            ImGui.SetClipboardText(friendCode ?? "ERROR_01");
            uiBuilder.AddNotification("Successfully copied id to clipboard", "Aether Remote", NotificationType.Success);
        }

        ImGui.SameLine();

        SharedUserInterfaces.BigTextCentered(friendCode ?? "ERROR_01", 8, SharedUserInterfaces.Gold);

        ImGui.SameLine(ImGui.GetWindowWidth() - 42);

        if (SharedUserInterfaces.IconButton(IconButtonArgs.Settings))
        {
            configWindow.IsOpen = true;
        }

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 50);

        ImGui.InputTextWithHint("###SearchFriend", "Search Friend", ref searchByFriendIdOrNoteInput, 
            AetherRemoteConstants.FriendCodeCharLimit);

        ImGui.SameLine();
        if (SharedUserInterfaces.IconButton(IconButtonArgs.AddFriend))
        {
            ImGui.OpenPopup(addFriendPopup.Name);
        }
        
        addFriendPopup.Draw();

        var filteredList = friendsListFilter.Filter(searchByFriendIdOrNoteInput);
        var filteredOnlineList = new List<Friend>();
        var filteredOfflineList = new List<Friend>();
        foreach (var friend in filteredList)
        {
            if (friend.Online)
                filteredOnlineList.Add(friend);
            else
                filteredOfflineList.Add(friend);
        }

        var friendListAreaSize = ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 52;
        ImGui.BeginChild("FriendListArea", new Vector2(0, friendListAreaSize));

        if (ImGui.TreeNodeEx("Online", TreeNodeFlags))
        {
            foreach (var friend in filteredOnlineList)
            {
                if (ImGui.Selectable($"###Selectable_{friend.FriendCode}", friend.Selected,
                    ImGuiSelectableFlags.SpanAllColumns, new Vector2(ImGui.GetWindowWidth() - 35, 0)))
                {
                    friend.Selected = !friend.Selected;
                }

                ImGui.SameLine();
                ImGui.SetCursorPosX(15);
                SharedUserInterfaces.Icon(FontAwesomeIcon.User, SharedUserInterfaces.Green);

                ImGui.SameLine();
                SharedUserInterfaces.ColorText(friend.NoteOrId, SharedUserInterfaces.White);

                ImGui.SameLine(ImGui.GetWindowWidth() - 25);
                var iconButton = MakeFriendConfigButtonArgs(friend.FriendCode);
                if (SharedUserInterfaces.IconButton(iconButton))
                {
                    friend.Selected = false;
                    editFriendPopup.Open(friend);
                }
            }

            editFriendPopup.Draw();

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Offline", TreeNodeFlags))
        {
            foreach(var friend in filteredOfflineList)
            {
                ImGui.SetCursorPosX(15);
                SharedUserInterfaces.Icon(FontAwesomeIcon.User, SharedUserInterfaces.Red);

                ImGui.SameLine();
                SharedUserInterfaces.ColorText(friend.NoteOrId, SharedUserInterfaces.Grey);

                ImGui.SameLine(ImGui.GetWindowWidth() - 25);
                var iconButton = MakeFriendConfigButtonArgs(friend.FriendCode);
                if (SharedUserInterfaces.IconButton(iconButton))
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

    private static IconButtonArgs MakeFriendConfigButtonArgs(string friendId)
    {
        return new IconButtonArgs()
        {
            Icon = FontAwesomeIcon.UserCog,
            Id = $"{friendId}-Settings",
            Size = new Vector2(22, 22),
            Offset = new Vector2(0, -5)
        };
    }
}
