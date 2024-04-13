using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteServer.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Services;

public interface INetworkService
{
    /// <summary>
    /// Wrapper for <see cref="StorageService.TryGetFriendCode(string)"/> <br/>
    /// Returns a friend code for associated secret or null if not found
    /// </summary>
    public string? TryGetRequesterFriendCode(string secret);

    /// <summary>
    /// Attempts to get a user from the online user list
    /// </summary>
    public User? TryGetOnlineUser(string friendCode);

    /// <summary>
    /// Registers a user as online.
    /// </summary>
    public GenericResult Register(string secret, string connectionId, List<Friend> friendList);

    /// <summary>
    /// Creates or updates a friend in user with provided secret's friend list.
    /// </summary>
    public GenericResult CreateOrUpdateFriend(User user, Friend friendToCreateOrUpdate);

    /// <summary>
    /// Deletes a friend in user with provided secret's friend list.
    /// </summary>
    public GenericResult DeleteFriend(User requesterUser, string friendCodeToDelete);

    #region Issue Commands

    /// <summary>
    /// Issues a speak command to a list of target friend codes
    /// </summary>
    public GenericResult IssueSpeakCommand(string issuerFriendCode, ChatMode channel, string message,
        string? extra, List<string> targetFriendCodes, IHubCallerClients clients);

    /// <summary>
    /// Issues an emote command to a list of target friend codes
    /// </summary>
    public GenericResult IssueEmoteCommand(string issuerFriendCode, string emote, List<string> targetFriendCodes,
        IHubCallerClients clients);

    /// <summary>
    /// Issues a become command to a list of target friend codes
    /// </summary>
    public GenericResult IssueBecomeCommand(string issuerFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType,
        List<string> targetFriendCodes, IHubCallerClients clients);

    #endregion
}
