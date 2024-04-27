using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Experimental.Tabs.Friends;

public class FriendsTab(FriendListProvider friendListProvider, NetworkProvider networkProvider,
    SecretProvider secretProvider, IPluginLog logger) : ITab
{
    // Constants
    private const ImGuiTableFlags FriendListTableFlags = ImGuiTableFlags.Borders;
    private static readonly Vector2 RoundButtonSize = new(40, 40);

    // Dependencies
    private readonly FriendListProvider friendListProvider = friendListProvider;
    private readonly NetworkProvider networkProvider = networkProvider;
    private readonly SecretProvider secretProvider = secretProvider;
    private readonly IPluginLog logger = logger;

    /// <summary>
    /// The string being referenced by the friend code input text
    /// </summary>
    private string friendCodeAddFriendInputText = "";

    /// <summary>
    /// The string being refered by the search input text
    /// </summary>
    private string friendCodeSearchInputText = "";

    /// <summary>
    /// The friend whose settings are currently being editted
    /// </summary>
    private Friend? friendBeingEditted = null;

    /// <summary>
    /// A list of friends to be deleted at the end of the draw event
    /// </summary>
    private readonly List<Friend> friendsToDelete = [];

    /// <summary>
    /// Threaded filter for searching your friend list
    /// </summary>
    private readonly ThreadedFilter<Friend> friendSearchFilter = new(friendListProvider.FriendList, FilterFriend);

    // Collection of permissions
    private string friendNote = string.Empty;

    private bool allowSpeak = false;
    private bool allowSay = false;
    private bool allowYell = false;
    private bool allowShout = false;
    private bool allowTell = false;
    private bool allowParty = false;
    private bool allowAlliance = false;
    private bool allowFreeCompany = false;
    private bool allowLinkshell = false;
    private bool allowCrossworldLinkshell = false;
    private bool allowPvPTeam = false;
    
    private bool allowEmote = false;

    private bool allowChangeAppearance = false;
    private bool allowChangeEquipment = false;

    public void Draw()
    {
        // Grab a reference to the style
        var style = ImGui.GetStyle();

        // The height of the footer containing the friend code input text and the add friend button
        var footerHeight = (ImGui.CalcTextSize("Add Friend").Y + (style.FramePadding.Y * 2) + style.ItemSpacing.Y) * 2;

        if (ImGui.BeginTabItem("Friends"))
        {
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref friendCodeSearchInputText, Constants.FriendNicknameCharLimit))
            {
                friendSearchFilter.Restart(friendCodeSearchInputText);
            }

            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();

            ImGui.SameLine();

            // Draw the settings area beside the search bar using the remaining space
            if (ImGui.BeginChild("FriendSettingsArea", Vector2.Zero, true))
            {
                DrawFriendSetting();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            // By setting the Y value as negative, the window will be that many pixels up from the bottom
            if (ImGui.BeginChild("FriendListArea", new Vector2(MainWindow.FriendListSize.X, -1 * footerHeight), true))
            {
                DrawFriendList();
                ImGui.EndChild();
            }

            ImGui.PopStyleVar();

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##FriendCodeInputText", "Friend Code", ref friendCodeAddFriendInputText, Constants.FriendCodeCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                AddFriendInInputText();
            }

            if (ImGui.Button("Add Friend", MainWindow.FriendListSize))
            {
                AddFriendInInputText();
            }

            ImGui.EndTabItem();
        }

        DeleteFriendsStep();
    }
    
    private void DrawFriendList()
    {
        if (ImGui.BeginTable("FriendListTable", 1, FriendListTableFlags))
        {
            foreach(var friend in friendListProvider.FriendList)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                SharedUserInterfaces.Icon(FontAwesomeIcon.User);
                ImGui.SameLine();

                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, SharedUserInterfaces.HoveredColorTheme);
                ImGui.PushStyleColor(ImGuiCol.Header, SharedUserInterfaces.SelectedColorTheme);
                if (ImGui.Selectable($"{friend.NoteOrFriendCode}", friendBeingEditted == friend, ImGuiSelectableFlags.SpanAllColumns))
                {
                    EditFriend(friend);
                }
                ImGui.PopStyleColor(2);
            }

            ImGui.EndTable();
        }
    }

    private void DrawFriendSetting()
    {
        // Store the value of the variable in case it changes mid-execution
        var friendBeingEdittedSnapshot = new Snapshot<Friend?>(friendBeingEditted);
        if (friendBeingEdittedSnapshot.Value == null)
        {
            SharedUserInterfaces.PushBigFont();
            var windowCenter = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 2);
            var textSize = ImGui.CalcTextSize("Select Friend");
            var cursorPos = new Vector2(windowCenter.X - (textSize.X / 2), windowCenter.Y - textSize.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Text("Select Friend");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Start by selecting a friend from the left");
        }
        else
        {
            SharedUserInterfaces.BigTextCentered($"Editing {friendBeingEdittedSnapshot.Value.FriendCode}");

            ImGui.SameLine();
            var deleteButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPosX(deleteButtonPosition.X);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, RoundButtonSize) && friendBeingEditted != null)
            {
                friendsToDelete.Add(friendBeingEditted);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Delete Friend");
                ImGui.EndTooltip();
            }

            ImGui.Separator();

            SharedUserInterfaces.TextCentered("Details");

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            ImGui.InputTextWithHint("Note##EditingFriendCode", "Note", ref friendNote, Constants.FriendCodeCharLimit);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("A note or 'nickname' to more easily identify a friend");
                ImGui.EndTooltip();
            }

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("General Permissions");

            ImGui.Checkbox("Allow force speak", ref allowSpeak);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Allow friend to force you to say things in chat");
                ImGui.EndTooltip();
            }

            ImGui.Indent();
            var allowSpeakSnapshot = new Snapshot<bool>(allowSpeak);
            if (allowSpeakSnapshot.Value == false) ImGui.BeginDisabled();
            if (ImGui.BeginTable("SpeakPermissionsTable", 2))
            {
                ImGui.TableNextColumn(); ImGui.Checkbox("Say", ref allowSay);
                ImGui.TableNextColumn(); ImGui.Checkbox("Yell", ref allowYell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Shout", ref allowShout);
                ImGui.TableNextColumn(); ImGui.Checkbox("Tell", ref allowTell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Party", ref allowParty);
                ImGui.TableNextColumn(); ImGui.Checkbox("Alliance", ref allowAlliance);
                ImGui.TableNextColumn(); ImGui.Checkbox("Free Company", ref allowFreeCompany);
                ImGui.TableNextColumn(); ImGui.Checkbox("PVP Team", ref allowPvPTeam);
                ImGui.TableNextColumn(); ImGui.Checkbox("Linkshells", ref allowLinkshell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Crosworld Linkshells", ref allowCrossworldLinkshell);
                ImGui.EndTable();
            }
            ImGui.Unindent();

            if (allowSpeakSnapshot.Value == false) ImGui.EndDisabled();

            ImGui.Checkbox("Allow force emote", ref allowEmote);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Allow friend to force you to perform emotes");
                ImGui.EndTooltip();
            }

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("Glamourer Permissions");

            ImGui.Checkbox("Allow change appearance", ref allowChangeAppearance);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Allow friend to change your character's appearance using glamourer");
                ImGui.EndTooltip();
            }

            ImGui.Checkbox("Allow change equipment", ref allowChangeEquipment);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Allow friend to change your character's equipment using glamourer");
                ImGui.EndTooltip();
            }

            var saveButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPos(saveButtonPosition);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, RoundButtonSize) && friendBeingEditted != null)
            {
                friendBeingEditted.Note = friendNote == string.Empty ? null : friendNote;
                friendBeingEditted.Preferences.AllowEmote = allowEmote;
                friendBeingEditted.Preferences.AllowSpeak = allowSpeak;
                friendBeingEditted.Preferences.AllowChangeAppearance = allowChangeAppearance;
                friendBeingEditted.Preferences.AllowChangeEquipment = allowChangeEquipment;
                friendBeingEditted.Preferences.AllowSay = allowSay;
                friendBeingEditted.Preferences.AllowYell = allowYell;
                friendBeingEditted.Preferences.AllowShout = allowShout;
                friendBeingEditted.Preferences.AllowTell = allowTell;
                friendBeingEditted.Preferences.AllowParty = allowParty;
                friendBeingEditted.Preferences.AllowAlliance = allowAlliance;
                friendBeingEditted.Preferences.AllowFreeCompany = allowFreeCompany;
                friendBeingEditted.Preferences.AllowLinkshell = allowLinkshell;
                friendBeingEditted.Preferences.AllowCrossworldLinkshell = allowCrossworldLinkshell;
                friendBeingEditted.Preferences.AllowPvPTeam = allowPvPTeam;

                friendListProvider.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Save Friend");
                ImGui.EndTooltip();
            }

            ImGui.PopStyleVar();
        }
    }

    private void DeleteFriendsStep()
    {
        if (friendsToDelete.Count <= 0)
            return;

        foreach(var friendToDelete in friendsToDelete)
        {
            friendListProvider.RemoveFriend(friendToDelete.FriendCode);
        }

        friendBeingEditted = null;
        friendsToDelete.Clear();
        friendListProvider.Save();
    }

    private void AddFriendInInputText()
    {
        if (friendCodeAddFriendInputText.Length <= 0)
            return;

        var addFriendResult = friendListProvider.AddFriend(friendCodeAddFriendInputText);
        if (addFriendResult == null)
            return;

        Task.Run(() => networkProvider.CreateOrUpdateFriend(secretProvider.Secret, addFriendResult));
    }

    private void EditFriend(Friend friend)
    {
        friendBeingEditted = friend;

        friendNote = friend.Note ?? string.Empty;
        allowSpeak = friend.Preferences.AllowSpeak;
        allowEmote = friend.Preferences.AllowEmote;
        allowChangeAppearance = friend.Preferences.AllowChangeAppearance;
        allowChangeEquipment = friend.Preferences.AllowChangeEquipment;
        allowSay = friend.Preferences.AllowSay;
        allowYell = friend.Preferences.AllowYell;
        allowShout = friend.Preferences.AllowShout;
        allowTell = friend.Preferences.AllowTell;
        allowParty = friend.Preferences.AllowParty;
        allowAlliance = friend.Preferences.AllowAlliance;
        allowFreeCompany = friend.Preferences.AllowFreeCompany;
        allowLinkshell = friend.Preferences.AllowLinkshell;
        allowCrossworldLinkshell = friend.Preferences.AllowCrossworldLinkshell;
        allowPvPTeam = friend.Preferences.AllowPvPTeam;
    }

    private static bool FilterFriend(Friend friend, string searchTerm)
    {
        var foundFriendCode = friend.FriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        if (foundFriendCode)
            return true;

        if (friend.Note == null)
            return false;

        return friend.Note.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
