using AetherRemoteCommon.Domain;
using System;

namespace AetherRemoteClient.Experimental;

public class ChatActionProcessedArgs : EventArgs
{
    public string Sender;
    public string Emote;
    public string Message;
    public ChatMode Channel;
    public string? Extra;
    public ChatActionType Type;

    public ChatActionProcessedArgs(string sender, string emote)
    {
        Sender = sender;
        Emote = emote;
        Message = string.Empty;
        Channel = ChatMode.Say;
        Extra = null;
        Type = ChatActionType.Emote;
    }

    public ChatActionProcessedArgs(string sender, string message, ChatMode channel, string? extra = null)
    {
        Sender = sender;
        Emote = string.Empty;
        Message = message;
        Channel = channel;
        Extra = extra;
        Type = ChatActionType.Speak;
    }
}
