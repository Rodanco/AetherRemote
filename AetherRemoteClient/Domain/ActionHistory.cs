using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public static class ActionHistory
{
    public static readonly List<LogEntry> Logs = new();

    public static void Log(string sender, string message, DateTime timestamp, LogType type)
    {
        var log = new LogEntry(sender, message, timestamp, type);
        Logs.Add(log);
    }
}

public struct LogEntry
{
    public string Sender;
    public string Message;
    public DateTime Timestamp;
    public LogType Type;

    public LogEntry(string sender, string message, DateTime timestamp, LogType type)
    {
        Sender = sender;
        Message = message;
        Timestamp = timestamp;
        Type = type;
    }
}

public enum LogType
{
    Inbound,
    Outbound,
    Error
}
