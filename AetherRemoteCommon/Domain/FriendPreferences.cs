using System.Text;

namespace AetherRemoteCommon.Domain.CommonFriendPreferences;

[Serializable]
public class FriendPreferences
{
    public bool AllowSpeak = true;
    public bool AllowEmote = true;
    public bool AllowChangeAppearance = true;
    public bool AllowChangeEquipment = true;
    public bool AllowSay = true;
    public bool AllowYell = true;
    public bool AllowShout = true;
    public bool AllowTell = true;
    public bool AllowParty = true;
    public bool AllowAlliance = true;
    public bool AllowFreeCompany = true;
    public bool AllowLinkshell = true;
    public bool AllowCrossworldLinkshell = true;
    public bool AllowPvPTeam = true;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("FriendPreferences[");
        sb.Append("AllowSpeak = ");
        sb.Append(AllowSpeak);
        sb.Append(", AllowEmote = ");
        sb.Append(AllowEmote);
        sb.Append(", AllowChangeAppearance = ");
        sb.Append(AllowChangeAppearance);
        sb.Append(", AllowChangeEquipment = ");
        sb.Append(AllowChangeEquipment);
        sb.Append(", AllowSay = ");
        sb.Append(AllowSay);
        sb.Append(", AllowYell = ");
        sb.Append(AllowYell);
        sb.Append(", AllowShout = ");
        sb.Append(AllowShout);
        sb.Append(", AllowTell = ");
        sb.Append(AllowTell);
        sb.Append(", AllowParty = ");
        sb.Append(AllowParty);
        sb.Append(", AllowAlliance = ");
        sb.Append(AllowAlliance);
        sb.Append(", AllowFreeCompany = ");
        sb.Append(AllowFreeCompany);
        sb.Append(", AllowLinkshell = ");
        sb.Append(AllowLinkshell);
        sb.Append(", AllowCrossworldLinkshell = ");
        sb.Append(AllowCrossworldLinkshell);
        sb.Append(", AllowPvPTeam = ");
        sb.Append(AllowPvPTeam);
        sb.Append(']');

        return sb.ToString();
    }
}
