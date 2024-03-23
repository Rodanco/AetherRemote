using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions;

public class SessionsTabExperimental : ITab
{
    // Injected
    private readonly FriendListProvider friendListProvider;
    private readonly IPluginLog logger;

    private readonly Random random;

    private readonly List<Session> sessions;

    public SessionsTabExperimental(FriendListProvider friendListProvider, IPluginLog logger)
    {
        this.friendListProvider = friendListProvider;
        this.logger = logger;

        random = new();

        sessions =
        [
            new(random.Next().ToString()),
            new(random.Next().ToString())
        ];
    }

    private string selectedSessionName = "";

    public void Draw()
    {
        if (ImGui.BeginTabItem("Sessions"))
        {
            // TODO: Scale this. 10% of screen minimum size or something, Iuno.
            var sessionButtonWidth = 40;
            var sessionButtonSize = new Vector2(sessionButtonWidth, sessionButtonWidth);

            // TODO: Scale this. It's currently button size + 8 * 2 (some padding value idk)
            if (ImGui.BeginChild("SessionListArea", new Vector2(sessionButtonWidth + 16, 0), true))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus, sessionButtonSize))
                {
                    sessions.Add(new Session(random.Next().ToString()));
                    // TODO: Assign more things, like a color, and a random icon
                }

                foreach (var session in sessions)
                {
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, sessionButtonSize, session.Id))
                    {
                        // TODO: Display session details
                        selectedSessionName = session.Name;
                        logger.Info($"{session.Name}");
                        logger.Info($"{selectedSessionName}");
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(session.Name);
                        ImGui.EndTooltip();
                    }

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX());
                }
                
                ImGui.PopStyleVar();

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("SessionArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.BigTextCentered(selectedSessionName);


                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    private class Session(string id, string? name = null)
    {
        /// <summary>
        /// Session Id
        /// </summary>
        public string Id = id;

        /// <summary>
        /// Name of the session.
        /// </summary>
        public string Name = name ?? id;

        /// <summary>
        /// List of friends locked into the session
        /// </summary>
        public List<Friend> FriendsInSession = [];
    }
}
