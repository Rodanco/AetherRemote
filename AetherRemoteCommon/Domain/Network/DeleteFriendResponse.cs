namespace AetherRemoteCommon.Domain.Network;

public class DeleteFriendResponse
{
    public bool Success;
    public string Message;

    public DeleteFriendResponse()
    {
        Success = false;
        Message = string.Empty;
    }

    public DeleteFriendResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeleteFriendResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
