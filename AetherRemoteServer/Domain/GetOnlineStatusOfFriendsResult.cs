namespace AetherRemoteServer.Domain;

/// <summary>
/// Result of calling <see cref="Services.INetworkService.GetOnlineStatusOfFriends(string)"/>
/// </summary>
public class GetOnlineStatusOfFriendsResult
{
    public bool Success;
    public string Message;
    public HashSet<string> OnlineFriendCodes;

    public GetOnlineStatusOfFriendsResult(bool success, string message, HashSet<string> onlineFriendCodes)
    {
        Success = success;
        Message = message;
        OnlineFriendCodes = onlineFriendCodes;
    }
}
