using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Translators;

public static class FriendTranslator
{
    public static AetherRemoteCommon.Domain.CommonFriend.Friend DomainToCommon(Friend friend)
    {
        var translated = new AetherRemoteCommon.Domain.CommonFriend.Friend();
        translated.FriendCode = friend.FriendCode;
        translated.Note = friend.Note;
        translated.Permissions = friend.Permissions;
        return translated;
    }

    public static Friend CommonToDomain(AetherRemoteCommon.Domain.CommonFriend.Friend friend)
    {
        return new Friend(friend.FriendCode, friend.Note, friend.Permissions);
    }

    public static List<AetherRemoteCommon.Domain.CommonFriend.Friend> DomainFriendListToCommon(List<Friend> friends)
    {
        var converted = new List<AetherRemoteCommon.Domain.CommonFriend.Friend>();
        foreach (var friend in friends)
        {
            converted.Add(DomainToCommon(friend));
        }
        return converted;
    }

    public static List<Friend> CommonFriendListToDomain(List<AetherRemoteCommon.Domain.CommonFriend.Friend> friends)
    {
        var converted = new List<Friend>();
        foreach (var friend in friends)
        {
            converted.Add(CommonToDomain(friend));
        }
        return converted;
    }
}
