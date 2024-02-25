using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Saves;
using AetherRemoteClient.Services.Save;
using System.Collections.Generic;

namespace AetherRemoteClient.Services;

public class SaveService
{
    private const string FriendListFilename = "friends.json";
    private const string SecretFilename = "secret.json";

    public string Secret
    {
        get => secretSave.Get.Secret;
        set
        {
            secretSave.Get.Secret = value;
        }
    }
    public List<Friend> FriendList => friendListSave.Get.Friends;

    private readonly SaveSystem<FriendListSave> friendListSave;
    private readonly SaveSystem<SecretSave> secretSave;

    public SaveService(Plugin plugin)
    {
        var fileDirectory = plugin.PluginInterface.ConfigDirectory.FullName;
        friendListSave = new SaveSystem<FriendListSave>(fileDirectory, FriendListFilename);
        secretSave = new SaveSystem<SecretSave>(fileDirectory, SecretFilename);
    }

    public void SaveAll()
    {
        SaveFriendList();
        SaveSecret();
    }

    public void SaveFriendList()
    {
        friendListSave.Save();
    }

    public void SaveSecret()
    {
        secretSave.Save();
    }
}
