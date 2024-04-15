using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
    // Injected
    private readonly FriendListProvider friendListProvider = friendListProvider;
    private readonly SecretProvider secretProvider = secretProvider;
    private readonly EmoteProvider emoteProvider = emoteProvider;
    private readonly NetworkProvider networkProvider = networkProvider;
    private readonly GlamourerAccessor glamourerAccessor = glamourerAccessor;
    private readonly IPluginLog logger = logger;
    private readonly ITargetManager targetManager = targetManager;

    private readonly Random random = new();
    private readonly List<Session> sessions = [];
    private readonly ThreadedFilter<string> emoteFilter = new(emoteProvider.Emotes, (emote, searchTerm) =>
        {
            return emote.Contains(searchTerm);
        });
    private Session? currentSession = null;

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

    private void DrawSessionList()
    {
        var style = ImGui.GetStyle();
        var sessionListArea = new Vector2(BigButtonSize.X + (style.WindowPadding.X * 2), 0);
        if (ImGui.BeginChild("SessionListArea", sessionListArea, true, ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plus, BigButtonSize))
            {
                var session = new Session(random.Next().ToString());
                currentSession = session;
                sessions.Add(session);
            }

            foreach (var session in sessions)
            {
                if (SharedUserInterfaces.IconButton(session.Icon, BigButtonSize, session.Id))
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
                    // TODO: Remove ! operator
                    foreach (var friend in currentSession!.TargetFriends)
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

        // TODO: Remove ! operator
        var result = await networkProvider.IssueSpeakCommand(secretProvider.Secret, currentSession!.TargetFriends, message, chatMode, extra);
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

        // TODO: Remove ! operator
        var result = await networkProvider.IssueEmoteCommand(secretProvider.Secret, currentSession!.TargetFriends, emote);
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

        // TODO: Remove ! operator
        var result = await networkProvider.IssueBecomeCommand(secretProvider.Secret, currentSession!.TargetFriends, glamourerData, glamourerApplyType);
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

    private class Session(string id, string? name = null, FontAwesomeIcon? icon = null, Vector4? color = null)
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
        /// The icon for the session
        /// </summary>
        // TODO: Calculate IconPool before hand, and pass in the result to avoid constant Random instantiation
        public FontAwesomeIcon Icon = icon ?? IconPool[new Random().Next(IconPool.Count)];

        /// <summary>
        /// The color of the icon
        /// </summary>         
        // TODO: Calculate ColorPool before hand, and pass in the result to avoid constant Random instantiation
        public Vector4 Color = color ?? ColorPool[new Random().Next(ColorPool.Count)];

        /// <summary>
        /// List of friends locked into the session
        /// </summary>
        public List<Friend> TargetFriends = [];

        // TODO: Move this out of session class
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

        // TODO: Move this out of session class
        public static readonly List<Vector4> ColorPool = [
            new Vector4(1, 1, 1, 1)
            ];

        public string TargetFriendsAsList()
        {
            return string.Join(", ", TargetFriends.Select(friend => friend.NoteOrFriendCode));
        }
    }
}
