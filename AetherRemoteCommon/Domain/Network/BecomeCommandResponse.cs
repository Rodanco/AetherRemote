namespace AetherRemoteCommon.Domain.Network;

public class BecomeCommandResponse
{
    public bool Success { get; private set; }
    public string Message { get; private set; }

    public BecomeCommandResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public BecomeCommandResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
