namespace AetherRemoteCommon.Domain.CommonFriendPermissions;

[Serializable]
public class FriendPermissions
{
    public bool AllowSpeak = false;
    public bool AllowEmote = false;
    public bool AllowChangeAppearance = false;
    public bool AllowChangeEquipment = false;
    public bool AllowSay = false;
    public bool AllowYell = false;
    public bool AllowShout = false;
    public bool AllowTell = false;
    public bool AllowParty = false;
    public bool AllowAlliance = false;
    public bool AllowFreeCompany = false;
    public bool AllowLinkshell = false;
    public bool AllowCrossworldLinkshell = false;
    public bool AllowPvPTeam = false;

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("FriendPermissions");
        sb.AddVariable("AllowSpeak", AllowSpeak);
        sb.AddVariable("AllowSay", AllowSay);
        sb.AddVariable("AllowYell", AllowYell);
        sb.AddVariable("AllowShout", AllowShout);
        sb.AddVariable("AllowTell", AllowTell);
        sb.AddVariable("AllowParty", AllowParty);
        sb.AddVariable("AllowAlliance", AllowAlliance);
        sb.AddVariable("AllowFreeCompany", AllowFreeCompany);
        sb.AddVariable("AllowLinkshell", AllowLinkshell);
        sb.AddVariable("AllowCrossworldLinkshell", AllowCrossworldLinkshell);
        sb.AddVariable("AllowPvPTeam", AllowPvPTeam);
        sb.AddVariable("AllowEmote", AllowEmote);
        sb.AddVariable("AllowChangeAppearance", AllowChangeAppearance);
        sb.AddVariable("AllowChangeEquipment", AllowChangeEquipment);
        return sb.ToString();
    }
}
