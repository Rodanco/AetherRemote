using System.Net.Sockets;

namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendResponse
{
    public bool Success { get; private set; }
    public string Message { get; private set; }

    public CreateOrUpdateFriendResponse()
    {
        Success = true;
        Message = string.Empty;
    }

    public CreateOrUpdateFriendResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override string ToString()
    {
        return $"CreateOrUpdateFriendResponse[Success={Success}, Message={Message}]";
    }
}
