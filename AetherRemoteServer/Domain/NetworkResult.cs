using AetherRemoteServer.Services;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Result of calling a method in <see cref="NetworkService"/>
/// </summary>
public class NetworkResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public NetworkResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
}
