namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandResponse
{
    public bool Success;
    public string Message;

    public EmoteCommandResponse()
    {
        Success = false;
        Message = string.Empty;
    }

    public EmoteCommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
