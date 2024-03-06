using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public static class ActionHistory
{
    public static readonly List<LogEntry> Logs = new();

    public static void Log(string sender, string message, DateTime timestamp)
    {
        var log = new LogEntry(sender, message, timestamp);
        Logs.Add(log);
    }
}

public struct LogEntry
{
    public string Sender;
    public string Message;
    public DateTime Timestamp;

    public LogEntry(string sender, string message, DateTime timestamp)
    {
        Sender = sender;
        Message = message;
        Timestamp = timestamp;
    }
}
