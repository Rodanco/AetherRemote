using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using System.Text;

namespace AetherRemoteCommon.Domain.Network;

public class AetherRemoteStringBuilder
{
    private readonly StringBuilder sb;

    /// <summary>
    /// Wrapper class designed to ease formatting for objects in ToString methods
    /// </summary>
    public AetherRemoteStringBuilder(string name)
    {
        sb = new StringBuilder();
        sb.Append(name);
        sb.Append('[');
    }

    public void AddVariable(string name, string? value)
    {
        sb.Append(name);
        sb.Append('=');
        sb.Append(value ?? string.Empty);
        sb.Append(',');
    }

    /// <summary>
    /// Extension for <see cref="bool"/>
    /// </summary>
    public void AddVariable(string name, bool value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="Friend"/>
    /// </summary>
    public void AddVariable(string name, Friend value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="List{String}"/>
    /// </summary>
    public void AddVariable(string name, List<string> values)
    {
        AddVariable(name, string.Join(", ", values));
    }

    /// <summary>
    /// Extension for <see cref="HashSet{String}"/>
    /// </summary>
    public void AddVariable(string name, HashSet<string>? values)
    {
        AddVariable(name, string.Join(", ", values ?? new()));
    }

    /// <summary>
    /// Extension for <see cref="List{Friend}"/>
    /// </summary>
    public void AddVariable(string name, List<Friend> values)
    {
        AddVariable(name, string.Join(", ", values));
    }

    /// <summary>
    /// Extension for <see cref="GlamourerApplyType"/>
    /// </summary>
    public void AddVariable(string name, GlamourerApplyType value)
    {
        AddVariable(name, value.ToString());
    }

    /// <summary>
    /// Extension for <see cref="ChatMode"/>
    /// </summary>
    public void AddVariable(string name, ChatMode value)
    {
        AddVariable(name, value.ToString());
    }

    public override string ToString()
    {
        sb.Length--;
        sb.Append(']');
        return sb.ToString();
    }
}
