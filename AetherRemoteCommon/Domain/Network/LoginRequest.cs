namespace AetherRemoteCommon.Domain.Network;

public class LoginRequest
{
    public string Secret { get; set; }
    public List<CommonFriend> FriendList { get; set; }

    public LoginRequest()
    {
        Secret = string.Empty;
        FriendList = new List<CommonFriend>();
    }

    public LoginRequest(string secret, List<CommonFriend> friendList)
    {
        Secret = secret;
        FriendList = friendList;
    }

    public override string ToString()
    {
        return $"LoginRequest[Secret={Secret}, FriendList={FriendList}]";
    }
}
