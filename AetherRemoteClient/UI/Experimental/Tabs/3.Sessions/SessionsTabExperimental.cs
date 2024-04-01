using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using AetherRemoteClient.Accessors.Glamourer;
using Dalamud.Game.ClientState.Objects;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions;

public class SessionsTabExperimental : ITab
{
    // Injected
    private readonly FriendListProvider friendListProvider;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;

    private readonly Random random;
    private readonly List<Session> sessions;
    private readonly ThreadedFilter<string> emoteFilter;
    private Session currentSession;

    private ChatMode chatMode = ChatMode.Say;
    private int shellNumber = 1;
    private string tellTarget = "";
    private string message = "";
    private string emote = "";
    private string glamourerData = "";
    private bool applyCustomization = true;
    private bool applyEquipment = true;

    private bool sessionFriendListCollapsed = true;

    private static Vector2 ButtonSize = new(30, 30);
    private static Vector2 BigButtonSize = new(40, 40);

    public SessionsTabExperimental(
        FriendListProvider friendListProvider,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        IPluginLog logger,
        ITargetManager targetManager)
    {
        this.friendListProvider = friendListProvider;
        this.glamourerAccessor = glamourerAccessor;
        this.logger = logger;
        this.targetManager = targetManager;

        random = new();

        sessions =
        [
            new(random.Next().ToString()),
            new(random.Next().ToString())
        ];

        currentSession = sessions[0];

        currentSession.FriendsInSession.Add(new Friend("Joe"));
        currentSession.FriendsInSession.Add(new Friend("Momma"));
        currentSession.FriendsInSession.Add(new Friend("Sugma"));

        emoteFilter = new(emoteProvider.Emotes, (emote, searchTerm) => 
        {
            return emote.Contains(searchTerm);
        });
    }

    public void Draw()
    {
        var style = ImGui.GetStyle();

        if (ImGui.BeginTabItem("Sessions"))
        {
            // TODO: Scale this. It's currently button size + 8 * 2 (some padding value idk)
            if (ImGui.BeginChild("SessionListArea", new Vector2(BigButtonSize.X + 16, 0), true))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);

                if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus, BigButtonSize))
                {
                    sessions.Add(new Session(random.Next().ToString()));
                    // TODO: Assign more things, like a color, and a random icon
                }

                foreach (var session in sessions)
                {
                    if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User, BigButtonSize, session.Id))
                    {
                        // TODO: Display session details
                        currentSession = session;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(session.Name);
                        ImGui.EndTooltip();
                    }
                }
                
                ImGui.PopStyleVar();

                ImGui.EndChild();
            }

            ImGui.SameLine();

            // TODO: Find a way to rename this to include the original name without making it overly verbose
            var snapshot = new Snapshot<bool>(sessionFriendListCollapsed);
            var sessionAreaSize = snapshot.Value ? Vector2.Zero : new Vector2(ImGui.GetWindowWidth() - (BigButtonSize.X + 16) - 160, 0);
            if (ImGui.BeginChild("SessionArea", sessionAreaSize, true))
            {
                SharedUserInterfaces.BigTextCentered(currentSession.Name);

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ButtonSize.X - style.ItemSpacing.X);
                var icon = snapshot.Value ? FontAwesomeIcon.UserFriends : FontAwesomeIcon.ArrowRight;
                if (SharedUserInterfaces.IconButton(icon, ButtonSize))
                {
                    // Don't include this in the snapshot, because this is
                    sessionFriendListCollapsed = !sessionFriendListCollapsed;
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    var text = snapshot.Value ? "Expand session list" : "Collapse session list";
                    ImGui.Text(text);
                    ImGui.EndTooltip();
                }

                DrawSpeakSection();

                DrawEmoteSection();

                DrawGlamourerSection();

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (!snapshot.Value)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

                if (ImGui.BeginChild("SessionFriendsArea", Vector2.Zero, true))
                {
                    if (ImGui.BeginTable("FriendListTable", 1, ImGuiTableFlags.Borders))
                    {
                        foreach (var friend in currentSession.FriendsInSession)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);

                            SharedUserInterfaces.Icon(FontAwesomeIcon.User);
                            ImGui.SameLine();

                            // TODO: Find a soft color that works nicely
                            ImGui.PushStyleColor(ImGuiCol.Header, ImGuiColors.DalamudGrey);
                            if (ImGui.Selectable($"{friend.NoteOrFriendCode}", false, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                
                            }
                            ImGui.PopStyleColor(1);
                        }

                        ImGui.EndTable();
                    }

                    ImGui.EndChild();
                }

                ImGui.PopStyleVar();
            }

            ImGui.EndTabItem();
        }
    }

    private void DrawSpeakSection()
    {
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumText(chatMode.ToCondensedString(), ImGuiColors.ParsedOrange);

        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(42);

            if (ImGui.BeginCombo("###LinkshellSelector", shellNumber.ToString()))
            {
                for (var i = 1; i < 9; i++)
                {
                    if (ImGui.Selectable(i.ToString(), shellNumber == i))
                    {
                        shellNumber = i;
                    }
                }

                ImGui.EndCombo();
            }
        }
        else if (chatMode == ChatMode.Tell)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("###TellTarget", "Target", ref tellTarget, AetherRemoteConstants.PlayerNameCharLimit);
        }

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Comment))
        {
            ImGui.OpenPopup("ChatModeSelector");
        }

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
            {
                if (ImGui.Selectable(mode.ToCondensedString(), mode == chatMode))
                {
                    chatMode = mode;
                }
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 100);
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 400, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            shouldProcessSpeakCommand = true;
        };

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50);
        if (ImGui.Button("Send"))
        {
            shouldProcessSpeakCommand = true;
        }

        if (shouldProcessSpeakCommand)
        {

        }
    }

    private void DrawEmoteSection()
    {
        var shouldProcessEmoteCommand = false;

        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        SharedUserInterfaces.ComboFilter("###EmoteSelector", ref emote, emoteFilter);

        ImGui.SameLine();

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Play))
        {
            shouldProcessEmoteCommand = true;
        }

        if (shouldProcessEmoteCommand)
        {

        }
    }

    private void DrawGlamourerSection()
    {
        var shouldProcessGlamourerCommand = false;

        SharedUserInterfaces.MediumText("Glamourer", glamourerAccessor.IsGlamourerInstalled ? ImGuiColors.ParsedOrange : ImGuiColors.DalamudGrey);
        if (!glamourerAccessor.IsGlamourerInstalled) ImGui.BeginDisabled();

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Crosshairs))
        {
            var target = targetManager.Target;
            if (target == null)
            {
                logger.Info("No target");
            }
            else
            {
                var data = glamourerAccessor.GetCustomization(target.Name.ToString());
                if (data == null)
                {
                    logger.Info("No glamourer data");
                }
                else
                {
                    glamourerData = data;
                }
            }
        }

        ImGui.SameLine();

        if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref glamourerData, AetherRemoteConstants.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            shouldProcessGlamourerCommand = true;
        }

        ImGui.SameLine();

        if (ImGui.Button("Transform"))
        {
            shouldProcessGlamourerCommand = true;
        }

        ImGui.Spacing();

        ImGui.Checkbox("Apply Customization", ref applyCustomization);
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 45);
        ImGui.Checkbox("Apply Equipment", ref applyEquipment);

        if (shouldProcessGlamourerCommand)
        {

        }

        if (!glamourerAccessor.IsGlamourerInstalled) ImGui.EndDisabled();
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
