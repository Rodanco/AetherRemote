namespace AetherRemoteClient.Experimental;

public class EmoteAction : IChatAction
{
    public readonly string Sender;
    public readonly string Emote;

    public EmoteAction(string sender, string emote)
    {
        Sender = sender; Emote = emote;
    }

    public string Build()
    {
        return $"/{Emote} motion";
    }

    public void Log()
    {

    }
}
