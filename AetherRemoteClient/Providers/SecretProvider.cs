using AetherRemoteClient.Domain;
using Dalamud.Plugin;
using System;

namespace AetherRemoteClient.Providers;

public class SecretProvider(DalamudPluginInterface pluginInterface)
{
    private const string FileName = "secret.json";
    private readonly SaveFile<SecretSave> saveSystem = new(pluginInterface.ConfigDirectory.FullName, FileName);

    public string Secret
    {
        get
        {
            return saveSystem.Get.Secret;
        }
        set
        {
            saveSystem.Get.Secret = value;
        }
    }

    public void Save()
    {
        saveSystem.Save();
    }

    [Serializable]
    private class SecretSave
    {
        public string Secret { get; set; } = string.Empty;
    }
}
