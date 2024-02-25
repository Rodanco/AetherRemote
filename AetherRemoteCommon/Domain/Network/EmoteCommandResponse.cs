namespace AetherRemoteCommon.Domain.Network;

public class EmoteCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public EmoteCommandResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public EmoteCommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        return $"EmoteCommandResponse[Success={Success}, Message={Message}]";
    }
}
