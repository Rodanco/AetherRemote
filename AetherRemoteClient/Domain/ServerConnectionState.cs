namespace AetherRemoteClient.Domain;

public enum ServerConnectionState
{
    Disconnected,
    Connecting,
    Reconnecting,
    Connected
}
