namespace AetherRemoteServer.Domain;

public class GenericResult
{
    public bool Success;
    public string Message;

    public GenericResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
}
