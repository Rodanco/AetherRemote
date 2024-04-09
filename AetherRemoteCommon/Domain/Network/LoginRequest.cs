using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network;

public class LoginRequest
{
    public string Secret { get; set; }
    public List<Friend> FriendList { get; set; }

    public LoginRequest()
    {
        Secret = string.Empty;
        FriendList = new List<Friend>();
    }

    public LoginRequest(string secret, List<Friend> friendList)
    {
        Secret = secret;
        FriendList = friendList;
    }

    public override string ToString()
    {
        return $"LoginRequest[Secret={Secret}, FriendList={FriendList}]";
    }
}
