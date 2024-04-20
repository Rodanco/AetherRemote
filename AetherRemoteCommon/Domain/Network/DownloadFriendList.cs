using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network.DownloadFriendList;

public class DownloadFriendListRequest
{
    public string Secret { get; set; }

    public DownloadFriendListRequest(string secrert)
    {
        Secret = secrert;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DownloadFriendListRequest");
        sb.AddVariable("Secret", Secret);
        return sb.ToString();
    }
}

public class DownloadFriendListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<Friend> FriendList { get; set; }

    public DownloadFriendListResponse(bool success, string message, List<Friend> friendList)
    {
        Success = success;
        Message = message;
        FriendList = friendList;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DownloadFriendListRequest");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}
