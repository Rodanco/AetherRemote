using AetherRemoteCommon.Domain;
using System.Text;

namespace AetherRemoteClient.Experimental;

public class SpeakAction : IChatAction
{
    public readonly string Sender;
    public readonly string Message;
    public readonly ChatMode Channel;
    public readonly string? Extra;

    public SpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        Sender = sender; Message = message; Channel = channel; Extra = extra;
    }

    public string Build()
    {
        var chatCommand = new StringBuilder();

        chatCommand.Append('/');
        chatCommand.Append(Channel.ToChatCommand());

        if (Channel == ChatMode.Linkshell || Channel == ChatMode.CrossworldLinkshell)
            chatCommand.Append(Extra);

        chatCommand.Append(' ');

        if (Channel == ChatMode.Tell)
            chatCommand.Append(Extra);

        chatCommand.Append(Message);

        return chatCommand.ToString();
    }

    public void Log()
    {
        // Call static class
    }
}
