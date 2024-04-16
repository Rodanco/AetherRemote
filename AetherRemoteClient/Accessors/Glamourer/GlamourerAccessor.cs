using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using System.Threading;
using System.Threading.Tasks;
using Glamourer.Api.IpcSubscribers;

namespace AetherRemoteClient.Accessors.Glamourer;

public class GlamourerAccessor : IDisposable
{
    private readonly IPluginLog logger;
    private readonly ICallGateSubscriber<(int, int)> glamourerApiVersions;
    private readonly ICallGateSubscriber<string, string?> glamourerGetAllCustomization;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyAll;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyOnlyEquipment;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyOnlyCustomization;

    private readonly ApiVersion glamourerApiVersion;
    private readonly ApplyState glamourerApplyState;
    private readonly GetStateBase64 glamourerGetStateBase;

    public bool IsGlamourerInstalled { get; private set; }

    private readonly CancellationTokenSource source = new();
    private readonly TimeSpan checkGlamourerApiInterval = TimeSpan.FromSeconds(15);

    public GlamourerAccessor(IPluginLog logger, DalamudPluginInterface pluginInterface)
    {
        this.logger = logger;

        var api_old = "Glamourer.ApiVersions";
        var api = "IGlamourerApiBase.ApiVersion";
        glamourerApiVersions = pluginInterface.GetIpcSubscriber<(int, int)>(api);
        glamourerGetAllCustomization = pluginInterface.GetIpcSubscriber<string, string?>("Glamourer.GetAllCustomization");
        glamourerApplyAll = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyAll");
        glamourerApplyOnlyEquipment = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyEquipment");
        glamourerApplyOnlyCustomization = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyCustomization");

        glamourerApiVersion = new(pluginInterface);
        glamourerApplyState = new(pluginInterface);
        glamourerGetStateBase = new(pluginInterface);

        PeriodicCheckGlamourerApi(() => { 
            IsGlamourerInstalled = CheckGlamourerInstalled();
            PluginLog.Error(IsGlamourerInstalled.ToString());
        }, source.Token);
    }

    private void SandBox()
    {
        

        // var someone = glamourerGetStateBase.Invoke();
    }


    /// <summary>
    /// Apply the glamourer design to a specified character.
    /// </summary>
    /// <param name="characterName"></param>
    /// <param name="glamourerData"></param>
    /// <param name="applyType"></param>
    /// <returns>If applying the design was successful</returns>
    public bool ApplyDesign(string characterName, string glamourerData, GlamourerApplyType applyType)
    {
        if (!IsGlamourerInstalled) return false;

        var operation = applyType switch
        {
            GlamourerApplyType.CustomizationAndEquipment => glamourerApplyAll,
            GlamourerApplyType.CustomizationOnly => glamourerApplyOnlyCustomization,
            GlamourerApplyType.EquipmentOnly => glamourerApplyOnlyEquipment,
            _ => glamourerApplyAll
        };

        try
        {
            operation.InvokeAction(glamourerData, characterName);
            return true;
        }
        catch
        {
            logger.Warning($"Glamourer::{operation} - Unable to apply glamourer data.");
            return false;
        }
    }

    /// <summary>
    /// Get a character's glamourer data.
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns>The character's glamourer data as a string.</returns>
    public string? GetCustomization(string characterName)
    {
        if (!IsGlamourerInstalled)
        {
            logger.Warning("Glamourer::GetAllCustomization - Glamourer not installed.");
            return null;
        }

        try
        {
            return glamourerGetAllCustomization.InvokeFunc(characterName);
        }
        catch
        {
            logger.Warning("Glamourer::GetAllCustomization - Unable to get glamourer customization details.");
            return null;
        }
    }

    /// <summary>
    /// Translates booleans into glamourer apply types.
    /// </summary>
    /// <returns></returns>
    public static GlamourerApplyType ConvertBoolsToApplyType(bool applyCustomization, bool applyEquipment)
    {
        return (applyEquipment, applyCustomization) switch
        {
            (true, true) => GlamourerApplyType.CustomizationAndEquipment,
            (false, false) => GlamourerApplyType.CustomizationAndEquipment,
            (true, false) => GlamourerApplyType.EquipmentOnly,
            (false, true) => GlamourerApplyType.CustomizationOnly,
        };
    }

    private void PeriodicCheckGlamourerApi(Action action, CancellationToken token)
    {
        if (action == null) return;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                action();
                await Task.Delay(checkGlamourerApiInterval, token);
            }
        }, token);
    }

    private bool CheckGlamourerInstalled()
    {
        var isGlamourerInstalled = false;
        try
        {
            var version = glamourerApiVersions.InvokeFunc();
            if (version.Item1 == 0 && version.Item2 >= 1)
            {
                isGlamourerInstalled = true;
            }

            return isGlamourerInstalled;
        }
        catch
        {
            return isGlamourerInstalled;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        source.Cancel();
    }
}
