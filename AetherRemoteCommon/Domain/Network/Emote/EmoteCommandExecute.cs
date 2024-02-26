using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandExecute : CommandExecute
{
    public string Emote { get; set; }

    public EmoteCommandExecute()
    {
        Emote = string.Empty;
    }

    public EmoteCommandExecute(string senderFriendCode, string emote) : base(senderFriendCode)
    {
        Emote = emote;
    }

    public override string ToString()
    {
        return $"EmoteCommandExecute=[SenderFriendCode={SenderFriendCode}, Emote={Emote}]";
    }
}
