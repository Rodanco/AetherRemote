using AetherRemoteClient.Domain;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

public class FriendListProvider
{
    private const string FileName = "friends.json";
    private readonly SaveFile<FriendListSave> saveSystem;

    public List<Friend> FriendList => saveSystem.Get.Friends;

    public FriendListProvider(DalamudPluginInterface pluginInterface)
    {
        saveSystem = new SaveFile<FriendListSave>(pluginInterface.ConfigDirectory.FullName, FileName);
    }

    public void Save()
    {
        saveSystem.Save();
    }

    public Friend? FindFriend(string friendCode)
    {
        return FriendList.Find(friend => friend.FriendCode == friendCode);
    }

    [Serializable]
    private class FriendListSave
    {
        public List<Friend> Friends { get; set; } = new();
    }
}
