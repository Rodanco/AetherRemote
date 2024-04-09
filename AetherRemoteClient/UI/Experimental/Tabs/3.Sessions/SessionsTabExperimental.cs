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
using System.Threading.Tasks;
using System.Text;
using System.Linq;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions;

public class SessionsTabExperimental : ITab
{
    // Injected
    private readonly FriendListProvider friendListProvider;
    private readonly SecretProvider secretProvider;
    private readonly EmoteProvider emoteProvider;
    private readonly NetworkProvider networkProvider;
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

    private bool showFriendsInSession = true;

    private static Vector2 ButtonSize = new(30, 30);
    private static Vector2 BigButtonSize = new(40, 40);
    private static readonly int LinkshellSelectorWidth = 42;

    public SessionsTabExperimental(
        FriendListProvider friendListProvider,
        SecretProvider secretProvider,
        NetworkProvider networkProvider,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        IPluginLog logger,
        ITargetManager targetManager)
    {
        this.friendListProvider = friendListProvider;
        this.secretProvider = secretProvider;
        this.networkProvider = networkProvider;
        this.emoteProvider = emoteProvider;
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

        currentSession.TargetFriends.Add(new Friend("Joe"));
        currentSession.TargetFriends.Add(new Friend("Momma"));
        currentSession.TargetFriends.Add(new Friend("Sugma"));

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
            var sessionListArea = new Vector2(BigButtonSize.X + (style.WindowPadding.X * 2), 0);
            if (ImGui.BeginChild("SessionListArea", sessionListArea, true))
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

            var showFriendsInSessionSnapshot = new Snapshot<bool>(showFriendsInSession);
            var sessionAreaSize = showFriendsInSessionSnapshot.Value ? Vector2.Zero : new Vector2(ImGui.GetWindowWidth() - (BigButtonSize.X + 16) - 160, 0);
            if (ImGui.BeginChild("SessionArea", sessionAreaSize, true))
            {
                SharedUserInterfaces.BigTextCentered(currentSession.Name);

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
                    var text = showFriendsInSessionSnapshot.Value ? "Expand session list" : "Collapse session list";
                    ImGui.Text(text);
                    ImGui.EndTooltip();
                }

                DrawSpeakSection();

                DrawEmoteSection();

                DrawGlamourerSection();

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
                        foreach (var friend in currentSession.TargetFriends)
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
                            ImGui.PopStyleColor();
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
            ImGui.SetNextItemWidth(LinkshellSelectorWidth);

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
            _ = ProcessSpeakCommand();
        }
    }

    private async Task ProcessSpeakCommand()
    {
        if (message.Length <= 0)
            return;

        string? extra = null;
        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            extra = shellNumber.ToString();
        }
        else if (chatMode == ChatMode.Tell)
        {
            if (tellTarget.Length > 0)
                extra = tellTarget;
        }

        var result = await networkProvider.IssueSpeakCommand(secretProvider.Secret, currentSession.TargetFriends, message, chatMode, extra);
        if (result.Success)
        {
            var sb = new StringBuilder();

            sb.Append("You made ");
            sb.Append(currentSession.TargetFriendsAsList());
            if (chatMode == ChatMode.Tell)
            {
                sb.Append("send a tell to ");
                sb.Append(extra);
                sb.Append(" saying: \"");
                sb.Append(message);
                sb.Append("\".");
            }
            else
            {
                sb.Append("say: \"");
                sb.Append(message);
                sb.Append("\" in ");
                sb.Append(chatMode.ToCondensedString());
                if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
                {
                    sb.Append(extra);
                }
                sb.Append('.');
            }

            AetherRemoteLogging.Log("Me", sb.ToString(), DateTime.Now, LogType.Sent);
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
            _ = ProcessEmoteCommand();
        }
    }

    private async Task ProcessEmoteCommand()
    {
        var validEmote = emoteProvider.Emotes.Contains(emote);
        if (validEmote == false)
            return;

        var result = await networkProvider.IssueEmoteCommand(secretProvider.Secret, currentSession.TargetFriends, emote);
        if (result.Success)
        {
            var sb = new StringBuilder();
            sb.Append("You made ");
            sb.Append(currentSession.TargetFriendsAsList());
            sb.Append(" do the ");
            sb.Append(emote);
            sb.Append(" emote.");

            AetherRemoteLogging.Log("Me", sb.ToString(), DateTime.Now, LogType.Sent);
        }
    }

    private void DrawGlamourerSection()
    {
        var shouldProcessGlamourerCommand = false;

        var isGlamourerInstalledSnapshot = new Snapshot<bool>(glamourerAccessor.IsGlamourerInstalled);
        SharedUserInterfaces.MediumText("Glamourer", isGlamourerInstalledSnapshot.Value ? ImGuiColors.ParsedOrange : ImGuiColors.DalamudGrey);
        if (!isGlamourerInstalledSnapshot.Value) ImGui.BeginDisabled();

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
            _ = ProcessGlamourerCommand();
        }

        if (!isGlamourerInstalledSnapshot.Value) ImGui.EndDisabled();
    }

    private async Task ProcessGlamourerCommand()
    {
        if (glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);
        var result = await networkProvider.IssueBecomeCommand(secretProvider.Secret, currentSession.TargetFriends, glamourerData, glamourerApplyType);
        if (result.Success)
        {
            var sb = new StringBuilder();
            sb.Append("You made ");
            sb.Append(currentSession.TargetFriendsAsList());
            switch (glamourerApplyType)
            {
                case GlamourerApplyType.EquipmentOnly:
                    sb.Append(" wear this outfit: [");
                    break;

                case GlamourerApplyType.CustomizationOnly:
                    sb.Append(" transform into this person: [");
                    break;

                case GlamourerApplyType.CustomizationAndEquipment:
                    sb.Append(" transform into a perfect copy of this person: [");
                    break;
            }

            sb.Append(glamourerData);
            sb.Append("].");

            AetherRemoteLogging.Log("Me", sb.ToString(), DateTime.Now, LogType.Sent);
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
        public List<Friend> TargetFriends = [];

        public string TargetFriendsAsList()
        {
            return string.Join(", ", TargetFriends.Select(friend => friend.NoteOrFriendCode));
        }
    }
}
