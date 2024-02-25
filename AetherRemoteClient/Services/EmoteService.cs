using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Services;

public class EmoteService
{
    public List<string> GetEmotes()
    {
        return emotes;
    }

    /// <summary>
    /// List containing all the current emote alias in the game.
    /// </summary>
    private readonly List<string> emotes;

    public EmoteService(Plugin plugin)
    {
        emotes = new();

        var emoteSheet = plugin.DataManager.Excel.GetSheet<Emote>();
        if (emoteSheet == null) return;

        for (uint i = 0; i < emoteSheet.RowCount; i++)
        {
            var emote = emoteSheet.GetRow(i);
            if (emote == null) continue;

            var command = emote?.TextCommand?.Value?.Command?.ToString();
            if (command.IsNullOrEmpty()) continue;
            emotes.Add(command[1..]);

            var shortCommand = emote?.TextCommand?.Value?.ShortCommand?.ToString();
            if (shortCommand.IsNullOrEmpty()) continue;
            emotes.Add(shortCommand[1..]);
        }

        emotes.Sort();
    }
}
