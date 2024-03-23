using AetherRemoteCommon.Domain;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Translators;

public static class FriendTranslator
{
    public static CommonFriend DomainToCommon(Friend friend)
    {
        var translated = new CommonFriend();
        translated.FriendCode = friend.FriendCode;
        translated.Note = friend.Note;
        translated.Preferences = friend.Preferences;
        return translated;
    }

    public static Friend CommonToDomain(CommonFriend friend)
    {
        return new Friend(friend.FriendCode, friend.Note, friend.Preferences);
    }

    public static List<CommonFriend> DomainFriendListToCommon(List<Friend> friends)
    {
        var baseFriends = new List<CommonFriend>();
        foreach (var friend in friends)
        {
            baseFriends.Add(DomainToCommon(friend));
        }
        return baseFriends;
    }
}
