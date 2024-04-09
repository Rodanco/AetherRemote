namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandResponse
{
    public bool Success;
    public string Message;

    public SpeakCommandResponse()
    {
        Success = false;
        Message = string.Empty;
    }

    public SpeakCommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
