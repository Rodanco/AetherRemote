namespace AetherRemoteCommon.Domain.Network;

public class DeleteFriendResponse
{
    public bool Success { get; private set; }
    public string Message { get; private set; }

    public DeleteFriendResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public DeleteFriendResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
