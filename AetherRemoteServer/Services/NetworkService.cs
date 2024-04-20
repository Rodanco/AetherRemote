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
        if (connectionMapping.TryGetValue(connectionId, out var secret))
        {
            connectionMapping.Remove(connectionId);
            registeredUsers.Remove(secret);
            return new ResultWithMessage(true);
        }

        return new ResultWithMessage(false, "ConnectionId does not map to a secret or may have already been terminated");
    }

    public ResultWithMessage Sync(string secret, string friendListHash)
    {
        if (registeredUsers.TryGetValue(secret, out var userData))
        {
            var hash = userData.Friends.GetHashCode();
            var hashesMatch = hash.ToString() == friendListHash;
            return new ResultWithMessage(hashesMatch);
        }

        return new ResultWithMessage(false, "Requester not logged in");
    }

    public ResultWithFriends FetchFriendList(string secret)
    {
        if (registeredUsers.TryGetValue(secret, out var userData))
            return new ResultWithFriends(true, string.Empty, userData.Friends);

        return new ResultWithFriends(false, "Requester not logged in");
    }
    
    public ResultWithMessage UpdateFriendList(string secret,  List<Friend> friendList)
    {
        if (registeredUsers.TryGetValue(secret, out var userData))
        {
            userData.Friends = friendList;
            return new ResultWithMessage(true);
        }

        return new ResultWithMessage(false, "Requester not logged in");
    }
}
