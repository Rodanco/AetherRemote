namespace AetherRemoteCommon.Domain;

public enum ChatMode
{
    Say,
    Yell,
    Shout,
    Tell,
    Party,
    Alliance,
    FreeCompany,
    Linkshell,
    CrossworldLinkshell,
    NoviceNetwork,
    PvPTeam,
}

public static class ChatModeTranslator
{
    public static string ToCondensedString(this ChatMode chatMode)
    {
        return chatMode switch
        {
            ChatMode.Say or
            ChatMode.Yell or
            ChatMode.Shout or
            ChatMode.Tell or
            ChatMode.Party or
            ChatMode.Alliance => chatMode.ToString(),
            ChatMode.Linkshell => "LS",
            ChatMode.FreeCompany => "Free Company",
            ChatMode.CrossworldLinkshell => "CWLS",
            ChatMode.NoviceNetwork => "Novice Network",
            ChatMode.PvPTeam => "PvP Team",
            _ => chatMode.ToString()
        }; ;
    }

    public static string ToFormattedString(this ChatMode chatMode)
    {
        return chatMode switch
        {
            ChatMode.Say or 
            ChatMode.Yell or 
            ChatMode.Shout or 
            ChatMode.Tell or 
            ChatMode.Party or 
            ChatMode.Alliance or 
            ChatMode.Linkshell => chatMode.ToString(),
            ChatMode.FreeCompany => "Free Company",
            ChatMode.CrossworldLinkshell => "Crossworld Linkshell",
            ChatMode.NoviceNetwork => "Novice Network",
            ChatMode.PvPTeam => "PvP Team",
            _ => chatMode.ToString()
        };
    }

    public static string ToChatCommand(this ChatMode chatMode)
    {
        return chatMode switch
        {
            ChatMode.Say => "s",
            ChatMode.Yell => "y",
            ChatMode.Shout => "sh",
            ChatMode.Tell => "t",
            ChatMode.Party => "p",
            ChatMode.Alliance => "a",
            ChatMode.FreeCompany => "fc",
            ChatMode.Linkshell => "l",
            ChatMode.CrossworldLinkshell => "cwl",
            ChatMode.NoviceNetwork => "n",
            ChatMode.PvPTeam => "pt",
            _ => throw new NotImplementedException()
        };
    }
}
