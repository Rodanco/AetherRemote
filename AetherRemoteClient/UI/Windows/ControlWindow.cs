using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AetherRemoteClient.UI.Windows;

public class ControlWindow : Window
{
    // Injected
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;

    // Accessors
    private readonly GlamourerAccessor glamourerAccessor;

    // Providers
    private readonly NetworkProvider networkProvider;
    private readonly EmoteProvider emoteProvider;
    private readonly SecretProvider secretProvider;

    // Services
    private readonly SessionManagerService sessionManagerService;

    // Data
    private readonly List<Friend> selectedFriends;
    private readonly ThreadedFilter<string> emoteFilter;
    private readonly int hash;
    
    private ChatMode chatMode = ChatMode.Say;
    private int shellNumber = 1;
    private bool applyCustomization = true;
    private bool applyEquipment = true;
    private string message = string.Empty;
    private string tellTarget = string.Empty;
    private string emote = string.Empty;
    private string glamourerData = string.Empty;

    private const ImGuiWindowFlags ControlWindowFlags =
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoResize;

    public ControlWindow(
        IPluginLog logger,
        ITargetManager targetManager,
        GlamourerAccessor glamourerAccessor,
        NetworkProvider networkProvider,
        EmoteProvider emoteProvider,
        SecretProvider secretProvider,
        SessionManagerService sessionManagerService,
        int hash,
        List<Friend> selectedFriends,
        string windowName) : base(windowName, ControlWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 250),
            MaximumSize = new Vector2(400, 250)
        };

        this.logger = logger;
        this.targetManager = targetManager;

        this.glamourerAccessor = glamourerAccessor;

        this.networkProvider = networkProvider;
        this.emoteProvider = emoteProvider;
        this.secretProvider = secretProvider;

        this.sessionManagerService = sessionManagerService;

        this.hash = hash;
        this.selectedFriends = selectedFriends;

        emoteFilter = new ThreadedFilter<string>(emoteProvider.Emotes, FilterEmote);
    }

    public override void Draw()
    {
        #region Message
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

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 30);
        SharedUserInterfaces.Icon(FontAwesomeIcon.UserLock);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(string.Join("\n", selectedFriends.Select(x => x.NoteOrFriendCode)));
            ImGui.EndTooltip();
        }

        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Comment))
        {
            ImGui.OpenPopup("ChatModeSelector");
        }

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode chatMode in Enum.GetValues(typeof(ChatMode)))
            {
                if (ImGui.Selectable(chatMode.ToCondensedString(), chatMode == this.chatMode))
                {
                    this.chatMode = chatMode;
                }
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 100);
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 400, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            ProcessSpeakCommand();
        };

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50);
        if(ImGui.Button("Send"))
        {
            ProcessSpeakCommand();
        }
        #endregion

        ImGui.Spacing();
        ImGui.Separator();

        #region Emote
        SharedUserInterfaces.MediumText("Emote", ImGuiColors.ParsedOrange);

        SharedUserInterfaces.ComboFilter("###EmoteSelector", ref emote, emoteFilter);
            
        ImGui.SameLine();
        
        if (SharedUserInterfaces.IconButtonScaled(FontAwesomeIcon.Play))
        {
            ProcessEmoteCommand();
        }

        #endregion

        ImGui.Spacing();
        ImGui.Separator();

        #region Glamourer

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
            ProcessGlamourerCommand();
        }

        ImGui.SameLine();

        if (ImGui.Button("Transform"))
        {
            ProcessGlamourerCommand();
        }

        ImGui.Spacing();

        ImGui.Checkbox("Apply Customization", ref applyCustomization);
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 45);
        ImGui.Checkbox("Apply Equipment", ref applyEquipment);

        if (!glamourerAccessor.IsGlamourerInstalled) ImGui.EndDisabled();

        #endregion
    }

    public override void OnClose()
    {
        sessionManagerService.EndSession(hash);
    }

    private async void ProcessEmoteCommand()
    {
        var validEmote = emoteProvider.Emotes.Contains(emote);
        if (validEmote == false)
            return;

        var result = await networkProvider.IssueEmoteCommand(secretProvider.Secret, selectedFriends, emote);
        if (result.Success)
        {
            var sb = new StringBuilder();
            sb.Append("You made ");
            sb.Append(string.Join(", ", selectedFriends.Select(friend => friend.NoteOrFriendCode)));
            sb.Append(" do the ");
            sb.Append(emote);
            sb.Append(" emote.");

            ActionHistory.Log("Me", sb.ToString(), DateTime.Now, LogType.Outbound);
        }
    }

    private async void ProcessSpeakCommand()
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

        var result = await networkProvider.IssueSpeakCommand(secretProvider.Secret, selectedFriends, message, chatMode, extra);
        if (result.Success)
        {
            var sb = new StringBuilder();

            sb.Append("You made ");
            sb.Append(string.Join(", ", selectedFriends.Select(friend => friend.NoteOrFriendCode)));
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

            ActionHistory.Log("Me", sb.ToString(), DateTime.Now, LogType.Outbound);
        }
    }

    private async void ProcessGlamourerCommand()
    {
        if (glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);
        var result = await networkProvider.IssueBecomeCommand(secretProvider.Secret, selectedFriends, glamourerData, glamourerApplyType);
        if (result.Success)
        {
            var sb = new StringBuilder();
            sb.Append("You made ");
            sb.Append(string.Join(", ", selectedFriends.Select(friend => friend.NoteOrFriendCode)));
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

            ActionHistory.Log("Me", sb.ToString(), DateTime.Now, LogType.Outbound);
        }
    }

    private static bool FilterEmote(string emote, string searchTerm)
    {
        return emote.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
