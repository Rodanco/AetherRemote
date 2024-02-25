using AetherRemoteCommon.Domain;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Translators;

public static class FriendTranslator
{
    public static BaseFriend DomainToCommon(Friend friend)
    {
        var translated = new BaseFriend();
        translated.FriendCode = friend.FriendCode;
        translated.Note = friend.Note;
        translated.Preferences = friend.Preferences;
        return translated;
    }

    public static Friend CommonToDomain(BaseFriend friend)
    {
        return new Friend(friend.FriendCode, friend.Note, friend.Preferences);
    }

    public static List<BaseFriend> DomainFriendListToCommon(List<Friend> friends)
    {
        var baseFriends = new List<BaseFriend>();
        foreach (var friend in friends)
        {
            baseFriends.Add(DomainToCommon(friend));
        }
        return baseFriends;
    }
}
