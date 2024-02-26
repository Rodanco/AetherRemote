namespace AetherRemoteCommon.Domain.Network.Base;

public class CommandExecute
{
    public string SenderFriendCode { get; set; }

    public CommandExecute()
    {
        SenderFriendCode = string.Empty;
    }

    public CommandExecute(string senderFriendCode)
    {
        SenderFriendCode = senderFriendCode;
    }

    public override string ToString()
    {
        return $"CommandExecute=[SenderFriendCode={SenderFriendCode}]";
    }
}
