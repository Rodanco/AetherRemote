using AetherRemoteClient.Domain;
using Dalamud.Plugin;
using System;

namespace AetherRemoteClient.Components;

public class SecretProvider
{
    private const string FileName = "secret.json";
    private readonly SaveSystem<SecretSave> saveSystem;

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

    public SecretProvider(DalamudPluginInterface pluginInterface)
    {
        saveSystem = new SaveSystem<SecretSave>(pluginInterface.ConfigDirectory.FullName, FileName);
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
