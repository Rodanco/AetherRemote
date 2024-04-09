namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandExecute
{
    public string SenderFriendCode;
    public string Emote;
    public EmoteCommandExecute()
    {
        SenderFriendCode = string.Empty;
        Emote = string.Empty;
    }

    public EmoteCommandExecute(string senderFriendCode, string emote)
    {
        SenderFriendCode = senderFriendCode;
        Emote = emote;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Emote", Emote);
        return sb.ToString();
    }
}
