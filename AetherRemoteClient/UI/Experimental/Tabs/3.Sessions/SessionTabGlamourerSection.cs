using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions.Glamourer;

public class SessionTabGlamourerSection(NetworkProvider networkProvider, SecretProvider secretProvider, GlamourerAccessor glamourerAccessor,
    IPluginLog logger, ITargetManager targetManager)
{
    private readonly NetworkProvider networkProvider = networkProvider;
    private readonly SecretProvider secretProvider = secretProvider;
    private readonly GlamourerAccessor glamourerAccessor = glamourerAccessor;
    private readonly IPluginLog logger = logger;
    private readonly ITargetManager targetManager = targetManager;

    private string glamourerData = "";
    private bool applyCustomization = true;
    private bool applyEquipment = true;
    private Session? currentSession = null;

    public void SetSession(Session newSession)
    {
        currentSession = newSession;
    }

    public void DrawGlamourerSection()
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
                var t = target.Address;
                

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
        if (currentSession == null)
            return;

        if (glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);

        var secret = secretProvider.Secret;
        var targets = currentSession.TargetFriends;
        var result = await networkProvider.IssueBecomeCommand(secret, targets, glamourerData, glamourerApplyType);
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
}
