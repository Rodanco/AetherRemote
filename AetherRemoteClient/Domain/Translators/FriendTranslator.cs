using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Translators;

public static class FriendTranslator
{
    public static AetherRemoteCommon.Domain.CommonFriend.Friend DomainToCommon(Friend friend)
    {
        var translated = new AetherRemoteCommon.Domain.CommonFriend.Friend();
        translated.FriendCode = friend.FriendCode;
        translated.Note = friend.Note;
        translated.Preferences = friend.Preferences;
        return translated;
    }

    public static Friend CommonToDomain(AetherRemoteCommon.Domain.CommonFriend.Friend friend)
    {
        return new Friend(friend.FriendCode, friend.Note, friend.Preferences);
    }

    public static List<AetherRemoteCommon.Domain.CommonFriend.Friend> DomainFriendListToCommon(List<Friend> friends)
    {
        var baseFriends = new List<AetherRemoteCommon.Domain.CommonFriend.Friend>();
        foreach (var friend in friends)
        {
            baseFriends.Add(DomainToCommon(friend));
        }
        return baseFriends;
    }
}
