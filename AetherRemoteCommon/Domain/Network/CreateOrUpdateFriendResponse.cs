namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendResponse
{
    public bool Success;
    public string Message;
    public bool Online;

    public CreateOrUpdateFriendResponse()
    {
        Success = false;
        Message = string.Empty;
        Online = false;
    }

    public CreateOrUpdateFriendResponse(bool success, string message, bool online = false)
    {
        Success = success;
        Message = message;
        Online = online;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
