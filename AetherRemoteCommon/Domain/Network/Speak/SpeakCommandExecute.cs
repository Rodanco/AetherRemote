using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandExecute : CommandExecute
{
    public string Message { get; set; }
    public ChatMode Channel { get; set; }
    public string? Extra { get; set; }

    public SpeakCommandExecute()
    {
        Message = string.Empty;
        Channel = ChatMode.Say;
        Extra = null;
    }

    public SpeakCommandExecute(string senderFriendCode, string message, ChatMode channel, string? extra)
        : base(senderFriendCode)
    {
        Message = message;
        Channel = channel;
        Extra = extra;
    }

    public override string ToString()
    {
        return $"SpeakCommandExecute=[SenderFriendCode={SenderFriendCode}, Message={Message}, Channel={Channel}, Extra={Extra}]";
    }
}
