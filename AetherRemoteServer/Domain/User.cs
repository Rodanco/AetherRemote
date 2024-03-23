using AetherRemoteCommon.Domain;

namespace AetherRemoteServer.Domain;

[Serializable]
public class User
{
    public string Secret = string.Empty;
    public string FriendCode = string.Empty;
    public List<CommonFriend> FriendList = new();

    [NonSerialized]
    public string ConnectionId = string.Empty;

    public User() { }
    public User(string secret, string friendCode, string connectionId, List<CommonFriend> friendList)
    {
        Secret = secret;
        FriendCode = friendCode;
        ConnectionId = connectionId;
        FriendList = friendList;
    }

    public bool IsFriendsWith(string friendCode)
    {
        Console.WriteLine("IsFriendsWith:" + string.Join(", ", FriendList));
        return FriendList.Any(friend => friend.FriendCode == friendCode);
    }
}
