using AetherRemoteClient.Components;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Windows.Popups;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
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
    private readonly FastFilter<Friend> friendsListFilter;

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

        // Data
        friendsListFilter = new FastFilter<Friend>(friendListService.FriendList);

        // Define Popups
        addFriendPopup = new AddFriendPopup(friendListService);
        editFriendPopup = new EditFriendPopup(friendListService);
        friendOptionSelectionPopup = new FriendOptionSelectionPopup();
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

        ImGui.InputTextWithHint("###SearchFriend", "Search Friend", ref searchByFriendIdOrNoteInput,
            AetherRemoteConstants.FriendCodeCharLimit);

        ImGui.SameLine();
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.UserPlus, addFriendButtonSize))
        {
            ImGui.OpenPopup(friendOptionSelectionPopup.Name);
        }

        friendOptionSelectionPopup.Draw();

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

        var friendListAreaSize = ImGui.GetWindowHeight() - ImGui.GetCursorPosY() - 50;
        ImGui.BeginChild("FriendListArea", new Vector2(0, friendListAreaSize), false, ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (ImGui.TreeNodeEx("Online", TreeNodeFlags))
        {
            foreach (var friend in filteredOnlineList)
            {
                var userSettingButtonSize = SharedUserInterfaces.CalculateIconButtonScaledSize(FontAwesomeIcon.UserCog);
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0.1f, 0.5f));

                var selectableId = $"{friend.NoteOrId}###{friend.FriendCode}";
                //var selectableSize = new Vector2(ImGui.GetWindowWidth() - (padding.X * 8) - userSettingButtonSize.X, userSettingButtonSize.Y);
                var selectableSize = new Vector2(0, 0);

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
            foreach(var friend in filteredOfflineList)
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

        var friendListCount = friendListService.SelectedFriends.Count <= 0;
        if (friendListCount) ImGui.BeginDisabled();
        if (ImGui.Button("Control", new Vector2(ImGui.GetWindowWidth() - 15, 40)))
        {
            sessionManagerService.StartSession(friendListService.SelectedFriends);
        }
        if (friendListCount) ImGui.EndDisabled();
    }
}
