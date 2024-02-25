using AetherRemoteClient.Domain;
using System;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain.Saves;

[Serializable]
public class FriendListSave
{
    public List<Friend> Friends { get; set; } = new();
}
