using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class ControlWindow : Window
{
    private readonly IconButtonArgs chatModeButtonArgs = new(FontAwesomeIcon.Comment, null, new Vector2(28, 25), new Vector2(5, 1));
    private readonly IconButtonArgs emoteButtonArgs = new(FontAwesomeIcon.Play, null, new Vector2(28, 25));
    private readonly IconButtonArgs getTargetGlamourerDataButtonArgs = new(FontAwesomeIcon.Crosshairs, null, new Vector2(28, 25), new Vector2(4, 0));

    private readonly List<Friend> selectedFriends;
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;
    private readonly EmoteService emoteService;
    private readonly NetworkService networkService;
    private readonly GlamourerAccessor glamourerAccessor;
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
        string windowName, 
        int hash,
        List<Friend> selectedFriends,
        IPluginLog logger,
        ITargetManager targetManager,
        EmoteService emoteService,
        NetworkService networkService,
        GlamourerAccessor glamourerAccessor) : base(windowName, ControlWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 250),
            MaximumSize = new Vector2(400, 250)
        };

        this.selectedFriends = selectedFriends;
        this.logger = logger;
        this.targetManager = targetManager;
        this.emoteService = emoteService;
        this.networkService = networkService;
        this.glamourerAccessor = glamourerAccessor;
        this.hash = hash;
    }

    public override void Draw()
    {
        // TODO: List selected friends

        #region Message
        SharedUserInterfaces.MediumText(chatMode.ToCondensedString(), SharedUserInterfaces.Gold);

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

        if (SharedUserInterfaces.IconButton(chatModeButtonArgs))
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
            ProcessChatCommand();
        };

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50);
        if(ImGui.Button("Send"))
        {
            ProcessChatCommand();
        }
        #endregion

        ImGui.Spacing();
        ImGui.Separator();

        #region Emote
        SharedUserInterfaces.MediumText("Emote", SharedUserInterfaces.Gold);

        SharedUserInterfaces.ComboFilter("###EmoteSelector", ref emote, new Domain.FastFilter<string>(emoteService.GetEmotes()));
        
        ImGui.SameLine();
        
        if (SharedUserInterfaces.IconButton(emoteButtonArgs))
        {
            networkService.Commands.IssueEmoteCommand(selectedFriends, emote);
        }

        #endregion

        ImGui.Spacing();
        ImGui.Separator();

        #region Glamourer

        SharedUserInterfaces.MediumText("Glamourer", glamourerAccessor.IsGlamourerInstalled ? SharedUserInterfaces.Gold : SharedUserInterfaces.Grey);

        if (!glamourerAccessor.IsGlamourerInstalled) ImGui.BeginDisabled();

        if (SharedUserInterfaces.IconButton(getTargetGlamourerDataButtonArgs))
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
        networkService.EndSession(hash);
    }

    private void ProcessChatCommand()
    {
        if (message.Length == 0) return;

        string? extra = null;
        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            extra = shellNumber.ToString();
        }
        else if (chatMode == ChatMode.Tell)
        {
            if (tellTarget.Length == 0) return;
            extra = tellTarget;
        }

        networkService.Commands.IssueSpeakCommand(selectedFriends, message, chatMode, extra);
    }

    private void ProcessGlamourerCommand()
    {
        if (glamourerData.Length == 0) return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);
        networkService.Commands.IssueBecomeCommand(selectedFriends, glamourerData, glamourerApplyType);
    }
}
