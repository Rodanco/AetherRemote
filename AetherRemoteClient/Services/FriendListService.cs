using AetherRemoteClient.Domain;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Services;

public class FriendListService
{
    private readonly NetworkService networkModule;
    private readonly SaveService saveService;
    private readonly IPluginLog logger;

    public List<Friend> Friends { get; private set; }
    public List<Friend> SelectedFriends => GetSelectedFriends();

    public FriendListService(Plugin plugin)
    {
        networkModule = plugin.NetworkService;
        saveService = plugin.SaveService;
        logger = plugin.Logger;

        if (Plugin.DeveloperMode)
        {
            Friends = new List<Friend>()
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
            Friends = saveService.FriendList;
        }
    }

    public async void AddFriend(Friend newFriend)
    {
        var alreadyFriends = Friends.Any(friend => friend.FriendCode == newFriend.FriendCode);
        if (alreadyFriends)
        {
            logger.Info("[Add Friend] Friend already exists.");
            return;
        }
        else
        {
            var result = await networkModule.Commands.CreateOrUpdateFriend(newFriend);
            if (result)
            {
                logger.Info("[Add Friend] Add friend successful.");
                Friends.Add(newFriend);
                saveService.SaveAll();
            }
            else
            {
                logger.Info("[Add Friend] Add friend unsuccessful.");
            }
        }
    }

    public async void UpdateFriend(Friend oldFriendData, Friend newFriendData)
    {
        var successful = await networkModule.Commands.CreateOrUpdateFriend(newFriendData);
        if (!successful)
        {
            logger.Info("[Update Friend] Friend update unsuccessful.");
            return;
        }

        for (var i = 0; i < Friends.Count; i++)
        {
            if (Friends[i].FriendCode == oldFriendData.FriendCode)
            {
                Friends[i] = newFriendData;
                return;
            }
        }

        logger.Warning("[Update Friend] Friend not found.");
    }

    public async void RemoveFriend(Friend friend)
    {
        var successful = await networkModule.Commands.DeleteFriend(friend);
        if (!successful)
        {
            logger.Info("[Remove Friend] Remove friend unsuccessful.");
            return;
        }

        for (var i = 0; i < Friends.Count; i++)
        {
            if (Friends[i].FriendCode == friend.FriendCode)
            {
                Friends.RemoveAt(i);
                return;
            }
        }

        logger.Warning("[Remove Friend] Friend not found.");
    }

    private List<Friend> GetSelectedFriends()
    {
        return Friends.Where(friend => friend.Selected).ToList();
    }
}
