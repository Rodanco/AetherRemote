using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Emote;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Glamourer;
using AetherRemoteClient.UI.Experimental.Tabs.Sessions.Speak;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
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

    private bool showFriendsInSession = true;

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

    public static readonly List<Vector4> ColorPool = [
        new Vector4(1, 1, 1, 1)
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
                var color = ColorPool[random.Next(ColorPool.Count)];
                var session = new Session(id, icon, color);
                sessions.Add(session);
                SetSession(session);
            }

            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, SharedUserInterfaces.HoveredColorTheme);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, SharedUserInterfaces.SelectedColorTheme);

            foreach (var session in sessions)
            {
                var currentSessionSnapshot = new Snapshot<Session>(currentSession?? new Session("tempSession",FontAwesomeIcon.Moon,ImGuiColors.DalamudGrey));

                if (session == currentSessionSnapshot.Value)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, SharedUserInterfaces.SelectedColorTheme);

                }

                if (SharedUserInterfaces.IconButton(session.Icon, BigButtonSize, session.Id))
                {
                    SetSession(session);
                }

                if (session == currentSessionSnapshot.Value)
                {
                    ImGui.PopStyleColor(1);

                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(session.Name);
                    ImGui.EndTooltip();
                }
            }

            ImGui.PopStyleColor(2);

            ImGui.PopStyleVar();

            ImGui.EndChild();
        }
    }

    private void DrawSessionArea()
    {
        var style = ImGui.GetStyle();
        var showFriendsInSessionSnapshot = new Snapshot<bool>(showFriendsInSession);
        var sessionAreaSize = showFriendsInSessionSnapshot.Value ? Vector2.Zero : new Vector2(ImGui.GetWindowWidth() - (BigButtonSize.X + 16) - 160, 0);
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
                ImGui.OpenPopup("DemoPopup");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Add friends to session");
                ImGui.EndTooltip();
            }

            DrawAddFriendPopup();

            ImGui.SameLine();

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ButtonSize.X - style.ItemSpacing.X);
            var icon = showFriendsInSessionSnapshot.Value ? FontAwesomeIcon.UserFriends : FontAwesomeIcon.ArrowRight;
            if (SharedUserInterfaces.IconButton(icon, ButtonSize))
            {
                showFriendsInSession = !showFriendsInSession;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                var text = showFriendsInSessionSnapshot.Value ? "Show" : "Hide" + " friends in session";
                ImGui.Text(text);
                ImGui.EndTooltip();
            }

            speakSection.DrawSpeakSection();
            emoteSection.DrawEmoteSection();
            glamourerSection.DrawGlamourerSection();

            ImGui.EndChild();
        }

        ImGui.SameLine();

        if (!showFriendsInSessionSnapshot.Value)
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
                        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, SharedUserInterfaces.HoveredColorTheme);
                        ImGui.PushStyleColor(ImGuiCol.HeaderActive, SharedUserInterfaces.SelectedColorTheme);
                        if (ImGui.Selectable($"{friend.NoteOrFriendCode}", false, ImGuiSelectableFlags.SpanAllColumns))
                        {

                        }
                        ImGui.PopStyleColor(2);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }

            ImGui.PopStyleVar();
        }
    }

    private void DrawAddFriendPopup()
    {
        if (ImGui.BeginPopup("DemoPopup"))
        {

            ImGui.Text("Hi!");

            ImGui.EndPopup();
        }
    }
}
