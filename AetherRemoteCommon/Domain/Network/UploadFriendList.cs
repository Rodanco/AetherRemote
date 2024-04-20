using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network.UploadFriendList;

public class UploadFriendListRequest
{
    public string Secret { get; set; }
    public List<Friend> FriendList { get; set; }

    public UploadFriendListRequest(string secrert, List<Friend> friendList)
    {
        Secret = secrert;
        FriendList = friendList;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DownloadFriendListRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}

public class UploadFriendListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public UploadFriendListResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DownloadFriendListRequest");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
