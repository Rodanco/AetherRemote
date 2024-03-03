using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AetherRemoteClient;

[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    /// Plugin configuration version
    /// </summary>
    public int Version { get; set; } = 0;

    /// <summary>
    /// Should the plugin automatically log the player in
    /// </summary>
    public bool AutoConnect { get; set; } = true;

    /// <summary>
    /// Should the plugin automatically attempt to reconnect to the server
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Should the plugin log almost everything in /xllog
    /// </summary>
    public bool VerboseLogging { get; set; } = true;

    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }
}
