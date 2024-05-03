using AetherRemoteCommon.Domain.CommonFriend;
using System.Security.Cryptography;
using System.Text;

namespace AetherRemoteCommon.Domain;

public static class AetherRemoteHash
{
    public static async Task<byte[]> ComputeFriendListHash(List<Friend> friendList)
    {
        var encode = Encoding.UTF8.GetBytes(string.Join("", friendList));
        return await Task.Run(() => { return MD5.HashData(encode); });
    }
}
