using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Emote;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Glamourer;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Speak;
using AetherRemoteCommon;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions;

public class SessionsTabExperimental(
    FriendListProvider friendListProvider,
    SecretProvider secretProvider,
    NetworkProvider networkProvider,
    EmoteProvider emoteProvider,
    GlamourerAccessor glamourerAccessor,
    IPluginLog logger,
    ITargetManager targetManager) : ITab
{
    private readonly Random random = new();
    private readonly List<Session> sessions = [];

    private Session? currentSession = null;

    private bool showFriendsInSession = false;

    private static Vector2 ButtonSize = new(30, 30);
    private static Vector2 BigButtonSize = new(40, 40);

    private readonly SessionTabSpeakSection speakSection = new(networkProvider, secretProvider);
    private readonly SessionTabEmoteSection emoteSection = new(networkProvider, secretProvider, emoteProvider);
    private readonly SessionTabGlamourerSection glamourerSection = new(networkProvider, secretProvider, glamourerAccessor, logger, targetManager);

    public static readonly List<FontAwesomeIcon> IconPool = [
        FontAwesomeIcon.Feather,
            FontAwesomeIcon.Heart,
            FontAwesomeIcon.Handcuffs,
            FontAwesomeIcon.Ankh,
            FontAwesomeIcon.Coffee,
            FontAwesomeIcon.Dove,
            FontAwesomeIcon.Moon,
            FontAwesomeIcon.IceCream,
            FontAwesomeIcon.Socks
        ];

    public void Draw()
    {
        if (ImGui.BeginTabItem("Sessions"))
        {
            // Draw Session List
            DrawSessionList();

            ImGui.SameLine();

            // Draw Session Area
            DrawSessionArea();

            ImGui.EndTabItem();
        }
    }

    private void SetSession(Session newSession)
    {
        currentSession = newSession;
        speakSection.SetSession(newSession);
        emoteSection.SetSession(newSession);
        glamourerSection.SetSession(newSession);
    }

    private void DrawSessionList()
    {
        var style = ImGui.GetStyle();
        var sessionListArea = new Vector2(BigButtonSize.X + (style.WindowPadding.X * 2), 0);
        if (ImGui.BeginChild("SessionListArea", sessionListArea, true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus, BigButtonSize))
            {
                var id = random.Next().ToString();
                var icon = IconPool[random.Next(IconPool.Count)];
                var session = new Session(id, icon);
                sessions.Add(session);
                SetSession(session);
            }

            foreach (var session in sessions)
            {
                var shouldSetSession = false;

                if (session == currentSession)
                    ImGui.PushStyleColor(ImGuiCol.Button, SharedUserInterfaces.SelectedColorTheme);

                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, SharedUserInterfaces.HoveredColorTheme);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, SharedUserInterfaces.SelectedColorTheme);
                if (SharedUserInterfaces.IconButton(session.Icon, BigButtonSize, session.Id))
                {
                    shouldSetSession = true;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(session.Name);
                    ImGui.EndTooltip();
                }
                ImGui.PopStyleColor(2);

                if (session == currentSession)
                    ImGui.PopStyleColor();

                if (shouldSetSession)
                    SetSession(session);
            }

            ImGui.PopStyleVar();

            ImGui.EndChild();
        }
    }

    private void DrawSessionArea()
    {
        var style = ImGui.GetStyle();
        var shouldSetShowFriendsInSession = false;
        var shouldSetPopup = false;

        Vector2 sessionAreaSize;
        if (currentSession == null)
        {
            sessionAreaSize = Vector2.Zero;
        }
        else
        {
            sessionAreaSize = showFriendsInSession ? new Vector2(ImGui.GetWindowWidth() - (BigButtonSize.X + 16) - 160, 0) : Vector2.Zero;
        }

        if (ImGui.BeginChild("SessionArea", sessionAreaSize, true))
        {
            if (currentSession == null)
            {
                SharedUserInterfaces.BigTextCentered("Create a session to begin");

                ImGui.EndChild();
                return;
            }

            SharedUserInterfaces.BigTextCentered(currentSession.Name);

            ImGui.SameLine();

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - (2 * (ButtonSize.X + style.ItemSpacing.X)));
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.UserPlus, ButtonSize))
            {
                // TODO: Make Popup to add friends
                ImGui.OpenPopup("AddFriendPopup");
                shouldSetPopup = true;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Add friends to session");
                ImGui.EndTooltip();
            }

            if (shouldSetPopup)
            {
                var width = popupWindowSize.X;
                var mouse = ImGui.GetMousePos();
                var pos = new Vector2(mouse.X - (width / 2), mouse.Y);

                InitiateAddFriendPopup();

                ImGui.SetNextWindowPos(pos);
            }

            DrawAddFriendPopup();

            ImGui.SameLine();

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ButtonSize.X - style.ItemSpacing.X);
            var icon = showFriendsInSession ? FontAwesomeIcon.ArrowRight : FontAwesomeIcon.UserFriends;
            if (SharedUserInterfaces.IconButton(icon, ButtonSize))
            {
                shouldSetShowFriendsInSession = true;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                var text = (showFriendsInSession ? "Hide" : "Show") + " friends in session";
                ImGui.Text(text);
                ImGui.EndTooltip();
            }

            speakSection.DrawSpeakSection();
            emoteSection.DrawEmoteSection();
            glamourerSection.DrawGlamourerSection();

            ImGui.EndChild();
        }

        ImGui.SameLine();

        if (showFriendsInSession)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            if (ImGui.BeginChild("SessionFriendsArea", Vector2.Zero, true))
            {
                if (ImGui.BeginTable("FriendListTable", 1, ImGuiTableFlags.Borders))
                {
                    // TODO: Remove ! operator
                    foreach (var friend in currentSession!.TargetFriends)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);

                        SharedUserInterfaces.Icon(FontAwesomeIcon.User);

                        ImGui.SameLine();

                        if (ImGui.Selectable($"{friend.NoteOrFriendCode}", false, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            // TODO: Validate if this is correct
                            // currentSession.TargetFriends.Remove(friend);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text("Right click to remove friend from session");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }

            ImGui.PopStyleVar();
        }

        if (shouldSetShowFriendsInSession)
            showFriendsInSession = !showFriendsInSession;
    }

    private string friendSearchString = "";
    private Vector2 popupWindowSize = new(200, 300);
    private ThreadedFilter<Friend> friendSearchFilter = new([], null);

    private void InitiateAddFriendPopup()
    {
        if (currentSession == null)
            return;

        var result = new List<Friend>();
        foreach(var friend in friendListProvider.FriendList)
        {
            logger.Info("friend: " + friend);
            var containedInTargetFriends = currentSession.TargetFriends.Any(targetFriend => targetFriend.FriendCode == friend.FriendCode);
            if (!containedInTargetFriends)
                result.Add(friend);
        }

        friendSearchFilter = new(result, FilterFriend);
    }

    private bool FilterFriend(Friend friend, string term)
    {
        var containedInNote = friend.NoteOrFriendCode.Contains(term, StringComparison.OrdinalIgnoreCase);
        var containedInFriendCode = friend.FriendCode.Contains(term, StringComparison.OrdinalIgnoreCase);
        return containedInNote || containedInFriendCode;
    }

    private void DrawAddFriendPopup()
    {
        if (currentSession == null)
            return;

        var pad = ImGui.GetStyle().WindowPadding;
        var width = popupWindowSize.X - (pad.X * 4);
        var childSize = popupWindowSize - new Vector2(pad.X * 2, pad.Y * 2);

        ImGui.SetNextWindowSize(popupWindowSize);

        if (ImGui.BeginPopup("AddFriendPopup", ImGuiWindowFlags.NoMove))
        {
            if (ImGui.BeginChild("AddFriendPopupChild", childSize, true))
            {
                SharedUserInterfaces.MediumTextCentered("Add Friends");

                ImGui.SetNextItemWidth(width);

                if (ImGui.InputTextWithHint("##AddFriendPopupSearchBar", "Search", ref friendSearchString, AetherRemoteConstants.FriendCodeCharLimit, ImGuiInputTextFlags.None))
                {
                   friendSearchFilter.Restart(friendSearchString);
                }

                var removeFromIndex = -1;
                for(var i = 0; i < friendSearchFilter.List.Count; i++)
                {
                    var friend = friendSearchFilter.List[i];
                    if (ImGui.Selectable(friend.NoteOrFriendCode))
                    {
                        removeFromIndex = i;
                        currentSession.TargetFriends.Add(friend);
                    }
                }

                if (removeFromIndex > -1)
                {
                    friendSearchFilter.List.RemoveAt(removeFromIndex);
                }

                ImGui.EndChild();
            }

            ImGui.EndPopup();
        }
    }
}
