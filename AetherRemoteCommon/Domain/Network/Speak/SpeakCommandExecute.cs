using AetherRemoteCommon.Domain.CommonChatMode;

namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandExecute
{
    public string SenderFriendCode;
    public string Message;
    public ChatMode Channel;
    public string? Extra;

    public SpeakCommandExecute()
    {
        SenderFriendCode = string.Empty;
        Message = string.Empty;
        Channel = ChatMode.Say;
        Extra = null;
    }

    public SpeakCommandExecute(string senderFriendCode, string message, ChatMode channel, string? extra)
    {
        SenderFriendCode = senderFriendCode;
        Message = message;
        Channel = channel;
        Extra = extra;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Channel", Channel);
        sb.AddVariable("Extra", Extra);
        return sb.ToString();
    }
}
