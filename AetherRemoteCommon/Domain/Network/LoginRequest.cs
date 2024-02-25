namespace AetherRemoteCommon.Domain.Network;

public class LoginRequest
{
    public string Secret { get; set; }
    public List<BaseFriend> FriendList { get; set; }

    public LoginRequest()
    {
        Secret = string.Empty;
        FriendList = new List<BaseFriend>();
    }

    public LoginRequest(string secret, List<BaseFriend> friendList)
    {
        Secret = secret;
        FriendList = friendList;
    }

    public override string ToString()
    {
        return $"LoginRequest[Secret={Secret}, FriendList={FriendList}]";
    }
}
