using AetherRemoteClient.Domain;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Providers;

public class FriendListProvider(DalamudPluginInterface pluginInterface)
{
    private const string FileName = "friends.json";
    private readonly SaveFile<FriendListSave> saveSystem = new SaveFile<FriendListSave>(pluginInterface.ConfigDirectory.FullName, FileName);

    private readonly List<Friend> devFriends =
    [
        new Friend("Demo1"),
        new Friend("Demo2"),
        new Friend("Demo3", "Nickname!"),
        new Friend("Demo4"),
        new Friend("Demo5"),
        new Friend("Demo6", "Catch Phrase!")
    ];

    public List<Friend> FriendList => Plugin.DeveloperMode ? devFriends : saveSystem.Get.Friends;

    public void Save()
    {
        saveSystem.Save();
    }

    /// <summary>
    /// Adds a friend code to the frined list. Checks if friend code already exists before doing so. 
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>The newly created friend, otherwise null.</returns>
    public Friend? AddFriend(string friendCode)
    {
        var friend = FindFriend(friendCode);
        if (friend == null)
        {
            friend = new Friend(friendCode);
            FriendList.Add(friend);
            return friend;
        }

        return friend;
    }

    public Friend? FindFriend(string friendCode)
    {
        return FriendList.Find(friend => friend.FriendCode == friendCode);
    }

    [Serializable]
    private class FriendListSave
    {
        public List<Friend> Friends { get; set; } = [];
    }
}
