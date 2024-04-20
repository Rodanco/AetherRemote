namespace AetherRemoteClient.Domain;

public class AsyncResult(bool success, string message = "")
{
    public bool Success = success;
    public string Message = message;
}
