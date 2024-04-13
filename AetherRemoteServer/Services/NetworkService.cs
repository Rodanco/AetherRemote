using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Domain;
using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Text;

namespace AetherRemoteServer.Services;

public class NetworkService : INetworkService
{
    private static readonly bool EnableVerboseLogging = true;

    private readonly StorageService storageService = new();
    private readonly List<User> onlineUsers = new();

    public string? TryGetRequesterFriendCode(string requesterSecret)
    {
        return storageService.TryGetFriendCode(requesterSecret);
    }

    public User? TryGetOnlineUser(string friendCode)
    {
        return onlineUsers.FirstOrDefault(user => user.FriendCode == friendCode);
    }

    public GenericResult Register(string secret, string connectionId, List<Friend> friendList)
    {
        var friendCode = storageService.TryGetFriendCode(secret);
        if (friendCode == null)
        {
            var logMessage = $"Provided secret {secret} does not match any records in the database";

            if (EnableVerboseLogging)
                Log(logMessage);

            return new GenericResult(false, logMessage);
        }

        var userAlreadyRegistered = onlineUsers.Any(user =>user.Secret == secret);
        if (userAlreadyRegistered)
        {
            var logMessage = $"Already registered secret {secret}";

            if (EnableVerboseLogging)
                Log(logMessage);

            return new GenericResult(false, logMessage);
        }

        onlineUsers.Add(new User(secret, friendCode, connectionId, friendList));
        return new GenericResult(true);
    }

    public GenericResult CreateOrUpdateFriend(User requesterUser, Friend friendToCreateOrUpdate)
    {
        var validFriendCode = storageService.IsValidFriendCode(friendToCreateOrUpdate.FriendCode);
        if (validFriendCode == false)
            return new GenericResult(false, $"FriendCode does not exist: {friendToCreateOrUpdate.FriendCode}");

        var index = requesterUser.FriendList.FindIndex(friend => friend.FriendCode == friendToCreateOrUpdate.FriendCode);
        if (index < 0)
        {
            requesterUser.FriendList.Add(friendToCreateOrUpdate);
        }
        else
        {
            requesterUser.FriendList[index] = friendToCreateOrUpdate;
        }

        return new GenericResult(true);
    }

    public GenericResult DeleteFriend(User requesterUser, string friendCodeToDelete)
    {
        var index = requesterUser.FriendList.FindIndex(f => f.FriendCode == friendCodeToDelete);
        if (index > -1)
            requesterUser.FriendList.RemoveAt(index);

        return new GenericResult(true);
    }

    public GenericResult IssueSpeakCommand(string issuerFriendCode, ChatMode channel, string message,
        string? extra, List<string> targetFriendCodes, IHubCallerClients clients)
    {
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
                return new GenericResult(false, ex.Message);
            }
        }

        return new GenericResult(true);
    }

    public GenericResult IssueEmoteCommand(string issuerFriendCode, string emote, List<string> targetFriendCodes, 
        IHubCallerClients clients)
    {
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
                return new GenericResult(false, ex.Message);
            }
        }

        return new GenericResult(true);
    }

    public GenericResult IssueBecomeCommand(string issuerFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType,
        List<string> targetFriendCodes, IHubCallerClients clients)
    {
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
                return new GenericResult(false, ex.Message);
            }
        }

        return new GenericResult(true);
    }

    private static void Log(string message)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[NetworkService] ");
        sb.AppendLine(message);
        Console.WriteLine(sb.ToString());
    }
}
