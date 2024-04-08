namespace AetherRemoteClient.Domain;

/// <summary>
/// Often times in UI, it is better to 'snapshot' a variable so that it does not change functionality mid-frame. This class <br/>
/// aims to do just that, but more as an acknowledgement or reminder than anything.
/// </summary>
public class Snapshot<T>(T value)
{
    public T Value { get; private set; } = value;
}
