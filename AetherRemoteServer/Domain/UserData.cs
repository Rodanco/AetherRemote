using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteServer.Domain;

[Serializable]
public class UserData
{
    public string FriendCode { get; set; } = string.Empty;
    public List<Friend> Friends { get; set; } = new();

    [NonSerialized]
    public string? ConnectionId = null;
}
