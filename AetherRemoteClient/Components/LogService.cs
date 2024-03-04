using AetherRemoteClient.Domain;
using AetherRemoteCommon.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AetherRemoteClient.Components;

public class LogService
{
    public readonly List<LogEntry> Logs;

    public LogService()
    {
        Logs = new List<LogEntry>();
    }

    public void LogBecomeInbound(string friendCode, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        var timestamp = DateTime.Now;
        var sb = BeginInboundLog(timestamp, friendCode);
        LogBecome(Type.Inbound, timestamp, sb, glamourerData, glamourerApplyType);
    }

    public void LogBecomeOutbound(List<Friend> targetFriends, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        var timestamp = DateTime.Now;
        var sb = BeginOutboundLog(timestamp, targetFriends);
        LogBecome(Type.Outbound, timestamp, sb, glamourerData, glamourerApplyType);
    }

    public void LogEmoteInbound(string friendCode, string emote)
    {
        var timestamp = DateTime.Now;
        var sb = BeginInboundLog(timestamp, friendCode);
        LogEmote(Type.Inbound, timestamp, sb, emote);
    }

    public void LogEmoteOutbound(List<Friend> targetFriends, string emote)
    {
        var timestamp = DateTime.Now;
        var sb = BeginOutboundLog(timestamp, targetFriends);
        LogEmote(Type.Outbound, timestamp, sb, emote);
    }

    public void LogSpeakInbound(string friendCode, string message, ChatMode channel, string? extra = null)
    {
        var timestamp = DateTime.Now;
        var sb = BeginInboundLog(timestamp, friendCode);
        LogSpeak(Type.Inbound, timestamp, sb, message, channel, extra);
    }

    public void LogSpeakOutbound(List<Friend> targetFriends, string message, ChatMode channel, string? extra = null)
    {
        var timestamp = DateTime.Now;
        var sb = BeginOutboundLog(timestamp, targetFriends);
        LogSpeak(Type.Outbound, timestamp, sb, message, channel, extra);
    }

    private void LogBecome(Type type, DateTime timestamp, StringBuilder sb, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        sb.Append("apply glamourer data: [");
        sb.Append(glamourerData);
        sb.Append("] with apply type: [");
        sb.Append(glamourerApplyType);
        sb.Append("].");

        var entry = new LogEntry(type, Command.Become, sb.ToString(), timestamp);
        Logs.Add(entry);
    }

    private void LogEmote(Type type, DateTime timestamp, StringBuilder sb, string emote)
    {
        sb.Append("do the ");
        sb.Append(emote);
        sb.Append(" emote.");

        var entry = new LogEntry(type, Command.Emote, sb.ToString(), timestamp);
        Logs.Add(entry);
    }

    private void LogSpeak(Type type, DateTime timestamp, StringBuilder sb, string message, ChatMode channel, string? extra = null)
    {
        if (channel == ChatMode.Tell)
        {
            sb.Append("send a Tell to ");
            sb.Append(extra);
            sb.Append(" saying \"");
            sb.Append(message);
            sb.Append('"');
        }
        else
        {
            sb.Append("say \"");
            sb.Append(message);
            sb.Append("\" in ");
            if (channel == ChatMode.Linkshell || channel == ChatMode.CrossworldLinkshell)
            {
                sb.Append(channel.ToCondensedString());
                sb.Append(extra);
            }
            else
            {
                sb.Append(channel);
            }

            sb.Append(" chat.");
        }

        var entry = new LogEntry(type, Command.Speak, sb.ToString(), timestamp);
        Logs.Add(entry);
    }

    /// <summary>
    /// Returns a <see cref="StringBuilder"/> with only a timestamp
    /// </summary>
    private static StringBuilder BeginLog(DateTime timestamp)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(timestamp);
        sb.Append("] ");
        return sb;
    }

    /// <summary>
    /// Returns a <see cref="StringBuilder"/> with a timestamp that ends in the phrase "made you "
    /// </summary>
    private static StringBuilder BeginInboundLog(DateTime timestamp, string friendCode)
    {
        var sb = BeginLog(timestamp);
        sb.Append(friendCode);
        sb.Append("Made you ");
        return sb;
    }

    /// <summary>
    /// Returns a <see cref="StringBuilder"/> with a timestamp that ends after listing your targets <see cref="Friend.NoteOrId"/>
    /// </summary>
    private static StringBuilder BeginOutboundLog(DateTime timestamp, List<Friend> targetFriends)
    {
        var sb = BeginLog(timestamp);
        sb.Append("You made [");
        sb.Append(string.Join(", ", targetFriends.Select(x => x.NoteOrId)));
        sb.Append("] ");
        return sb;
    }

    public struct LogEntry
    {
        public Type Type;
        public Command Command;
        public DateTime Timestamp;
        public string Message;

        public LogEntry(Type type, Command command, string message, DateTime timestamp)
        {
            Type = type;
            Command = command;
            Message = message;
            Timestamp = timestamp;
        }
    }

    public enum Type
    {
        Inbound,
        Outbound
    }

    public enum Command
    {
        Become,
        Emote,
        Speak
    }
}
