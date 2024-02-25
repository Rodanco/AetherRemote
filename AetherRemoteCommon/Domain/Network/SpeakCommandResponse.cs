namespace AetherRemoteCommon.Domain.Network;

public class SpeakCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public SpeakCommandResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public SpeakCommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        return $"SpeakCommandResponse[Success={Success}, Message={Message}]";
    }
}
