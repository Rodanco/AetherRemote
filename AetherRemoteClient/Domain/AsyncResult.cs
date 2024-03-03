namespace AetherRemoteClient.Domain;

public class AsyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public AsyncResult()
    {
        Success = false;
        Message = string.Empty;
    }

    public AsyncResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public static readonly AsyncResult Successful = new(true, string.Empty);
    public static readonly AsyncResult Failure = new(false, string.Empty);
}
