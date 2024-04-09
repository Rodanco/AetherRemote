namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendResponse
{
    public bool Success;
    public string Message;
    // TODO: Provide online status when changing something about a friend

    public CreateOrUpdateFriendResponse()
    {
        Success = false;
        Message = string.Empty;
    }

    public CreateOrUpdateFriendResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
