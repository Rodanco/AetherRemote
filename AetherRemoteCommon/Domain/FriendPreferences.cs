namespace AetherRemoteCommon.Domain;

[Serializable]
public class FriendPreferences
{
    public bool AllowSpeak = true;
    public bool AllowEmote = true;
    public bool AllowChangeAppearance = true;
    public bool AllowChangeEquipment = true;

    public override string ToString()
    {
        return $"FriendPreferences[AllowSpeak={AllowSpeak}, AllowEmote={AllowEmote}, AllowChangeAppearance={AllowChangeAppearance}, AllowChangeEquipment={AllowChangeEquipment}]";
    }
}
