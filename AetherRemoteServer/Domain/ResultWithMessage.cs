namespace AetherRemoteServer.Domain;

public class ResultWithMessage
{
    public bool Success;
    public string Message;
    public string Extra;

    public ResultWithMessage(bool success, string message = "", string extra = "")
    {
        Success = success;
        Message = message;
        Extra = extra;
    }
}
