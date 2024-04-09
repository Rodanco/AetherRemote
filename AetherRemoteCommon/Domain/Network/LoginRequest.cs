using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network;

public class LoginRequest
{
    public string Secret;
    public List<Friend> FriendList;

    public LoginRequest()
    {
        Secret = string.Empty;
        FriendList = new();
    }

    public LoginRequest(string secret, List<Friend> friendList)
    {
        Secret = secret;
        FriendList = friendList;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}
