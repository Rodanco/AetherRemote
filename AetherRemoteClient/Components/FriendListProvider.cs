using AetherRemoteClient.Domain;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Components;

public class FriendListProvider
{
    private const string FileName = "friends.json";
    private readonly SaveSystem<FriendListSave> saveSystem;

    public List<Friend> FriendList => saveSystem.Get.Friends;

    public FriendListProvider(DalamudPluginInterface pluginInterface)
    {
        saveSystem = new SaveSystem<FriendListSave>(pluginInterface.ConfigDirectory.FullName, FileName);
    }

    public void Save()
    {
        saveSystem.Save();
    }

    [Serializable]
    private class FriendListSave
    {
        public List<Friend> Friends { get; set; } = new();
    }
}
