using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services;

public class FriendListService
{
    // Injected
    private readonly IPluginLog logger;

    // Providers
    private readonly NetworkProvider networkProvider;
    private readonly FriendListProvider friendListProvider;
    private readonly SecretProvider secretProvider;
    
    public List<Friend> FriendList { get; private set; }
    public List<Friend> SelectedFriends => GetSelectedFriends();

    public FriendListService(IPluginLog logger, NetworkProvider networkProvider, 
        FriendListProvider friendListProvider, SecretProvider secretProvider)
    {
        this.logger = logger;
        this.networkProvider = networkProvider;
        this.friendListProvider = friendListProvider;
        this.secretProvider = secretProvider;

        if (Plugin.DeveloperMode)
        {
            FriendList = new List<Friend>()
            {
                new Friend("Demo10") { Online = true },
                new Friend("Demo20") { Online = true },
                new Friend("Demo30") { Online = true },
                new Friend("Demo11") { Online = true },
                new Friend("Demo22") { Online = true },
                new Friend("Demo33") { Online = true },
                new Friend("Demo4") { Online = false },
                new Friend("Demo5") { Online = false },
            };
        }
        else
        {
            FriendList = friendListProvider.FriendList;
        }
    }

    public async Task<AsyncResult> AddFriend(Friend newFriend)
    {
        var alreadyFriends = FriendList.Any(friend => friend.FriendCode == newFriend.FriendCode);
        if (alreadyFriends)
        {
            return new AsyncResult(false, "Friend already exists");
        }
        else
        {
            var result = await networkProvider.CreateOrUpdateFriend(secretProvider.Secret, newFriend);
            if (result.Success)
            {
                FriendList.Add(newFriend);
                friendListProvider.Save();
            }

            return result;
        }
    }

    public async void UpdateFriend(Friend oldFriendData, Friend newFriendData)
    {
        var successful = await networkProvider.CreateOrUpdateFriend(secretProvider.Secret, newFriendData);
        if (!successful.Success)
        {
            logger.Info("[Update Friend] Friend update unsuccessful.");
            return;
        }

        for (var i = 0; i < FriendList.Count; i++)
        {
            if (FriendList[i].FriendCode == oldFriendData.FriendCode)
            {
                FriendList[i] = newFriendData;
                return;
            }
        }

        logger.Warning("[Update Friend] Friend not found.");
    }

    public async void RemoveFriend(Friend friend)
    {
        var successful = await networkProvider.DeleteFriend(secretProvider.Secret,friend);
        if (!successful.Success)
        {
            logger.Info("[Remove Friend] Remove friend unsuccessful.");
            return;
        }

        for (var i = 0; i < FriendList.Count; i++)
        {
            if (FriendList[i].FriendCode == friend.FriendCode)
            {
                FriendList.RemoveAt(i);
                return;
            }
        }

        logger.Warning("[Remove Friend] Friend not found.");
    }

    public bool IsOnFriendList(Friend friendInQuestion)
    {
        return FriendList.Any(friend => friend.FriendCode == friendInQuestion.FriendCode);
    }

    private List<Friend> GetSelectedFriends()
    {
        return FriendList.Where(friend => friend.Selected).ToList();
    }
}
