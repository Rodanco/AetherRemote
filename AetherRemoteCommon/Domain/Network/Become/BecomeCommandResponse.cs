namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandResponse
{
    public bool Success;
    public string Message;

    public BecomeCommandResponse()
    {
        Success = false;
        Message = string.Empty;
    }

    public BecomeCommandResponse(bool success, string message)
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
