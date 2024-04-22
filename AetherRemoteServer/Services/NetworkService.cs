using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteServer.Domain;

namespace AetherRemoteServer.Services;

public class NetworkService
{
    // Mapping ConnectionId -> Secret
    private readonly Dictionary<string, string> connectionMapping = new();

    // Mapping Secret -> UserData
    private readonly Dictionary<string, UserData> registeredUsers = new();
    private readonly StorageService storageService = new();

    public ResultWithMessage Login(string connectionId, string secret)
    {
        // Validate Secret
        var userData = storageService.TryGetUserData(secret);
        if (userData == null)
            return new ResultWithMessage(false, "Invalid Secret");

        // Register ConnectionId -> Secret
        if (connectionMapping.ContainsKey(connectionId))
            return new ResultWithMessage(false, "Connection Id already mapped to a secret. Catastrophic failure! Alert a developer!");

        connectionMapping.Add(connectionId, secret);

        // Register Secret -> UserData
        if (registeredUsers.ContainsKey(secret))
            return new ResultWithMessage(false, "Already Registered");

        registeredUsers.Add(secret, userData);

        return new ResultWithMessage(true, "Success", userData.FriendCode);
    }

    public ResultWithMessage Logout(string connectionId)
    {
        if (!connectionMapping.TryGetValue(connectionId, out var secret))
            return new ResultWithMessage(false, "ConnectionId does not map to a secret or may have already been terminated");

        connectionMapping.Remove(connectionId);
        registeredUsers.Remove(secret);
        return new ResultWithMessage(true);
    }

    public ResultWithMessage Sync(string secret, string friendListHash)
    {
        if (!registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithMessage(false, "Requester not logged in");

        var hash = userData.Friends.GetHashCode().ToString();
        var hashesMatch = hash == friendListHash;
        return new ResultWithMessage(hashesMatch);
    }

    public ResultWithFriends FetchFriendList(string secret)
    {
        if (!registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithFriends(false, "Requester not logged in");

        return new ResultWithFriends(true, string.Empty, userData.Friends);
    }
    
    public ResultWithMessage UpdateFriendList(string secret,  List<Friend> friendList)
    {
        if (!registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithMessage(false, "Requester not logged in");

        userData.Friends = friendList;
        return new ResultWithMessage(true);
    }

    public ResultWithMessage CreateOrUpdateFriend(string secret, Friend friend)
    {
        if (!registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithMessage(false, "Requester not logged in");

        var index = userData.Friends.FindIndex(fr => fr.FriendCode == friend.FriendCode);
        if (index < 0)
        {
            userData.Friends.Add(friend); // Create
        }
        else
        {
            userData.Friends[index] = friend; // Update
        }

        return new ResultWithMessage(true);
    }

    public ResultWithMessage DeleteFriend(string secret, string friendCode)
    {
        if (!registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithMessage(false, "Requester not logged in");

        var result = new ResultWithMessage(true);
        var index = userData.Friends.FindIndex(friend => friend.FriendCode == friendCode);
        if (index < 0)
        {
            result.Message = "Friend not found";
        }
        else
        {
            userData.Friends.RemoveAt(index);
        }

        return result;
    }
}
