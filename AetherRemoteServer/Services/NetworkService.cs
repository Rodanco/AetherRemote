using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Services;

public interface INetworkService
{
    /// <summary>
    /// Wrapper for <see cref="StorageService.IsValidSecret(string)"/> <br/>
    /// </summary>
    public bool IsValidSecret(string secret);

    /// <summary>
    /// Wrapper for <see cref="StorageService.GetFriendCode(string)"/> <br/>
    /// Returns a friend code for associated secret or null if not found
    /// </summary>
    public string? GetFriendCode(string secret);

    /// <summary>
    /// Registers a user as online.<br/>
    /// </summary>
    public NetworkResult Register(string secret, string connectionId, List<BaseFriend> friendList);

    /// <summary>
    /// Creates or updates a friend in user with provided secret's friend list. <br/>
    /// </summary>
    public NetworkResult CreateOrUpdateFriend(string secret, BaseFriend friendToCreateOrUpdate);

    /// <summary>
    /// Deletes a friend in user with provided secret's friend list. <br/>
    /// </summary>
    public NetworkResult DeleteFriend(string secret, string friendCodeToDelete);

    /// <summary>
    /// Issues a speak command to a list of target friend codes
    /// </summary>
    public NetworkResult IssueSpeakCommand(string secret, ChatMode channel, string message,
        string? extra, List<string> targetFriendCodes, IHubCallerClients clients);

    /// <summary>
    /// Issues an emote command to a list of target friend codes
    /// </summary>
    public NetworkResult IssueEmoteCommand(string secret, string emote, List<string> targetFriendCodes,
        IHubCallerClients clients);

    /// <summary>
    /// Issues a become command to a list of target friend codes
    /// </summary>
    public NetworkResult IssueBecomeCommand(string secret, string glamourerData, GlamourerApplyType glamourerApplyType,
        List<string> targetFriendCodes, IHubCallerClients clients);
}

public class NetworkService : INetworkService
{
    private readonly StorageService storageService = new();
    private readonly List<User> onlineUsers = new();

    public bool IsValidSecret(string secret)
    {
        return GetFriendCode(secret) != null;
    }

    public string? GetFriendCode(string secret)
    {
        return storageService.GetFriendCode(secret);
    }

    public NetworkResult Register(string secret, string connectionId, List<BaseFriend> friendList)
    {
        var friendCode = storageService.GetFriendCode(secret);
        if (friendCode == null)
            return new NetworkResult(false, $"Could not find friend code for provided secret: {secret}");

        foreach (var user in onlineUsers)
        {
            if (user.Secret == secret)
                return new NetworkResult(false, $"Already registered secret: {secret}");
        }

        onlineUsers.Add(new User(secret, friendCode, connectionId, friendList));
        return new NetworkResult(true);
    }

    public NetworkResult CreateOrUpdateFriend(string secret, BaseFriend friendToCreateOrUpdate)
    {
        var user = onlineUsers.FirstOrDefault(u => u.Secret == secret);
        if (user == null)
            return new NetworkResult(false, $"Could not find online user who matches provided secret: {secret}");

        var result = new NetworkResult(true);
        var index = user.FriendList.FindIndex(f => f.FriendCode == friendToCreateOrUpdate.FriendCode);
        if (index < 0)
        {
            user.FriendList.Add(friendToCreateOrUpdate);
            result.Message = "Create successful";
        }
        else
        {
            user.FriendList[index] = friendToCreateOrUpdate;
            result.Message = "Update successful";
        }

        return result;
    }

    public NetworkResult DeleteFriend(string secret, string friendCodeToDelete)
    {
        var user = onlineUsers.FirstOrDefault(u => u.Secret == secret);
        if (user == null)
            return new NetworkResult(false, $"Could not find online user who matches provided secret: {secret}");

        var index = user.FriendList.FindIndex(f => f.FriendCode == friendCodeToDelete);
        if (index > -1)
            user.FriendList.RemoveAt(index);

        return new NetworkResult(true);
    }

    public NetworkResult IssueSpeakCommand(string secret, ChatMode channel, string message,
        string? extra, List<string> targetFriendCodes, IHubCallerClients clients)
    {
        var issuerFriendCode = storageService.GetFriendCode(secret);
        if (issuerFriendCode == null)
            return new NetworkResult(false, $"Could not find friend code for provided secret: {secret}");

        foreach (var targetFriendCode in targetFriendCodes)
        {
            var targetUser = TryGetOnlineUser(targetFriendCode);
            if (targetUser == null)
                continue; // Target is offline

            var issuerOnTargetFriendList = targetUser.IsFriendsWith(issuerFriendCode);
            if (issuerOnTargetFriendList == false)
                continue; // Issuer is not on target's friend list

            try
            {
                var execute = new SpeakCommandExecute(issuerFriendCode, message, channel, extra);
                clients.Client(targetUser.ConnectionId).SendAsync(AetherRemoteConstants.ApiSpeak, execute);
            }
            catch (Exception ex)
            {
                return new NetworkResult(false, ex.Message);
            }
        }

        return new NetworkResult(true);
    }

    public NetworkResult IssueEmoteCommand(string secret, string emote, List<string> targetFriendCodes, 
        IHubCallerClients clients)
    {
        var issuerFriendCode = storageService.GetFriendCode(secret);
        if (issuerFriendCode == null)
            return new NetworkResult(false, $"Could not find friend code for provided secret: {secret}");

        foreach (var targetFriendCode in targetFriendCodes)
        {
            var targetUser = TryGetOnlineUser(targetFriendCode);
            if (targetUser == null)
            {
                Console.WriteLine("Ugh12.");
                continue; // Target is offline
            }

            var issuerOnTargetFriendList = targetUser.IsFriendsWith(issuerFriendCode);
            if (issuerOnTargetFriendList == false)
            {
                Console.WriteLine("Ugh.");
                continue; // Issuer is not on target's friend list
            }

            try
            {
                var execute = new EmoteCommandExecute(issuerFriendCode, emote);
                clients.Client(targetUser.ConnectionId).SendAsync(AetherRemoteConstants.ApiEmote, execute);
            }
            catch (Exception ex)
            {
                return new NetworkResult(false, ex.Message);
            }
        }

        return new NetworkResult(true);
    }

    public NetworkResult IssueBecomeCommand(string secret, string glamourerData, GlamourerApplyType glamourerApplyType,
        List<string> targetFriendCodes, IHubCallerClients clients)
    {
        var issuerFriendCode = storageService.GetFriendCode(secret);
        if (issuerFriendCode == null)
            return new NetworkResult(false, $"Could not find friend code for provided secret: {secret}");

        foreach (var targetFriendCode in targetFriendCodes)
        {
            var targetUser = TryGetOnlineUser(targetFriendCode);
            if (targetUser == null)
                continue; // Target is offline

            var issuerOnTargetFriendList = targetUser.IsFriendsWith(issuerFriendCode);
            if (issuerOnTargetFriendList == false)
                continue; // Issuer is not on target's friend list

            try
            {
                var execute = new BecomeCommandExecute(issuerFriendCode, glamourerData, glamourerApplyType);
                clients.Client(targetUser.ConnectionId).SendAsync(AetherRemoteConstants.ApiBecome, execute);
            }
            catch (Exception ex)
            {
                return new NetworkResult(false, ex.Message);
            }
        }

        return new NetworkResult(true);
    }

    private User? TryGetOnlineUser(string friendCode)
    {
        return onlineUsers.FirstOrDefault(user => user.FriendCode == friendCode);
    }
}
