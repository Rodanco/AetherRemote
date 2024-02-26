namespace AetherRemoteCommon.Domain.Network.Base;

public class CommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public CommandResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public CommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        return $"CommandResponse[Success={Success}, Message={Message}]";
    }
}
