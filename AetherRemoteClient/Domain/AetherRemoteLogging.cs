using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

public static class AetherRemoteLogging
{
    public static readonly List<LogEntry> Logs = Plugin.DeveloperMode ?
    [
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
        new LogEntry("me", "test", DateTime.Now, LogType.Info),
    ] : [];

    public static void Log(string sender, string message, DateTime timestamp, LogType type)
    {
        var log = new LogEntry(sender, message, timestamp, type);
        Logs.Add(log);
    }
}

public struct LogEntry(string sender, string message, DateTime timestamp, LogType type)
{
    public string Sender = sender;
    public string Message = message;
    public DateTime Timestamp = timestamp;
    public LogType Type = type;
}

public enum LogType
{
    Sent,
    Recieved,
    Info,
    Error
}
