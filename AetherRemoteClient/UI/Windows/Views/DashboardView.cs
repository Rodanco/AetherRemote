using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Popups;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows.Views;

public class DashboardView : IWindow
{
    // Inject
    private readonly IPluginLog logger;
    private readonly UiBuilder uiBuilder;
    private readonly ConfigWindow configWindow;
    private readonly Configuration configuration;
    private readonly MainWindow mainWindow;
    private readonly LogWindow logWindow;

    // Provides
    private readonly NetworkProvider networkProvider;

    // Services
    private readonly FriendListService friendListService;
    private readonly SessionManagerService sessionManagerService;

    // Filter
    private readonly ThreadedFilter<Friend> friendListFilter;

    // Variable used to search friends list
    private string searchFriendInput = string.Empty;

    // Popup Windows
    private readonly AddFriendPopup addFriendPopup;
    private readonly EditFriendPopup editFriendPopup;
    private readonly FriendOptionSelectionPopup friendOptionSelectionPopup;

    private const ImGuiTreeNodeFlags TreeNodeFlags =
            ImGuiTreeNodeFlags.SpanFullWidth |
            ImGuiTreeNodeFlags.DefaultOpen |
            ImGuiTreeNodeFlags.FramePadding;

    private const int SelectableSize = 28;

    public DashboardView(
        IPluginLog logger,
        DalamudPluginInterface pluginInterface,
        MainWindow mainWindow,
        ConfigWindow configWindow,
        LogWindow logWindow,
        Configuration configuration,
        NetworkProvider networkProvider,
        FriendListService friendListService,
        SessionManagerService sessionManagerService)
    {
        // Inject
        this.logger = logger;
        this.uiBuilder = pluginInterface.UiBuilder;
        this.mainWindow = mainWindow;
        this.configWindow = configWindow;
        this.logWindow = logWindow;
        this.configuration = configuration;
        this.networkProvider = networkProvider;
        this.friendListService = friendListService;
        this.sessionManagerService = sessionManagerService;

        // Define Popups
        addFriendPopup = new AddFriendPopup(friendListService);
        editFriendPopup = new EditFriendPopup(friendListService);
        friendOptionSelectionPopup = new FriendOptionSelectionPopup();

        friendListFilter = new ThreadedFilter<Friend>(friendListService.FriendList, FilterFriend);
    }

    public void Draw()
    {
        if (!Plugin.DeveloperMode && networkProvider.Connection.State == HubConnectionState.Disconnected)
        {
            mainWindow.SetCurrentViewToLogin();
            return;
        }

        var padding = ImGui.GetStyle().FramePadding;

        var friendCode = networkProvider.FriendCode ?? "Unknown";

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Copy, 1.5f))
        {
            ImGui.SetClipboardText(friendCode);
            uiBuilder.AddNotification("Successfully copied id to clipboard", "Aether Remote", NotificationType.Success);
        }

        ImGui.SameLine();

        // 35 is the size of the buttons, 8 is the default padding. Yay magic numbers!
        var workingSpace = ImGui.GetWindowWidth() - ((2 * 35) - (4 * padding.X));
        SharedUserInterfaces.DynamicTextCentered(friendCode, workingSpace, SharedUserInterfaces.Gold);

        var configWindowButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.Wrench, 1.5f);
        ImGui.SameLine(ImGui.GetWindowWidth() - configWindowButtonSize.X - 8);
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.MagnifyingGlassArrowRight, 1.5f))
        {
            logWindow.IsOpen = !logWindow.IsOpen;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Open log window");
            ImGui.EndTooltip();
        }

        ImGui.Separator();
        ImGui.Spacing();

        var addFriendButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.UserPlus);
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - addFriendButtonSize.X - (padding.X * 6));

        if (ImGui.InputTextWithHint("###SearchFriend", "Search Friend", ref searchFriendInput, AetherRemoteConstants.FriendCodeCharLimit))
            friendListFilter.Restart(searchFriendInput);

        ImGui.SameLine();
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.UserPlus, addFriendButtonSize))
        {
            ImGui.OpenPopup(addFriendPopup.Name);
        }

        addFriendPopup.Draw();

        var filteredOnlineList = new List<Friend>();
        var filteredOfflineList = new List<Friend>();
        foreach (var friend in friendListFilter.List)
        {
            if (friend.Online)
                filteredOnlineList.Add(friend);
            else
                filteredOfflineList.Add(friend);
        }

        var friendListAreaSize = ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 50;
        ImGui.BeginChild("FriendListArea", new Vector2(0, friendListAreaSize), false, ImGuiWindowFlags.None);

        if (ImGui.TreeNodeEx("Online", TreeNodeFlags))
        {
            foreach (var friend in filteredOnlineList)
            {
                DrawFriendInDashboard(friend);
            }

            editFriendPopup.Draw();
            ImGui.TreePop();
        }


        if (ImGui.TreeNodeEx("Offline", TreeNodeFlags))
        {
            foreach (var friend in filteredOfflineList)
            {
                DrawFriendInDashboard(friend, true);
            }

            editFriendPopup.Draw();
            ImGui.TreePop();
        }

        ImGui.EndChild();

        var friendListCount = friendListService.SelectedFriends.Count <= 0;
        if (friendListCount) ImGui.BeginDisabled();
        if (ImGui.Button("Control", new Vector2(ImGui.GetWindowWidth() - 15, 40)))
        {
            sessionManagerService.StartSession(friendListService.SelectedFriends);
        }
        if (friendListCount) ImGui.EndDisabled();
    }

    private void DrawFriendInDashboard(Friend friend, bool offline = false)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

        var beginCursorY = ImGui.GetCursorPosY();
        if (offline) ImGui.BeginDisabled();
        if (ImGui.Selectable($"###{friend.FriendCode}", friend.Selected, ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, SelectableSize)))
        {
            friend.Selected = !friend.Selected;
        }
        if (offline) ImGui.EndDisabled();

        ImGui.SetItemAllowOverlap();
        var selectableWidth = ImGui.GetItemRectSize().X;
        var endCursorY = ImGui.GetCursorPosY();
        var iconSize = SharedUserInterfaces.CalculateIconSize(FontAwesomeIcon.User);

        ImGui.SetCursorPosX(12);
        ImGui.SetCursorPosY(beginCursorY + (SelectableSize / 2) - (iconSize.Y / 2));
        SharedUserInterfaces.Icon(FontAwesomeIcon.User, offline ? SharedUserInterfaces.Red : SharedUserInterfaces.Green);
        
        ImGui.SameLine(0, 8);
        ImGui.Text(friend.NoteOrId);

        ImGui.SameLine();
        ImGui.SetCursorPos(new Vector2(selectableWidth - SelectableSize, beginCursorY));

        var padding = SelectableSize * 0.2f;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (padding / 2));
        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.UserCog, new Vector2(SelectableSize - padding, SelectableSize - padding), $"{friend.NoteOrId}-Settings"))
        {
            editFriendPopup.Open(friend);
        }

        ImGui.SetCursorPosY(endCursorY);
        ImGui.PopStyleVar(2);
    }

    private static bool FilterFriend(Friend friend, string searchTerm)
    {
        return friend.NoteOrId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
