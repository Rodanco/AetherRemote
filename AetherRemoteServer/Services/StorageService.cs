using System.Text;
using System.Text.Json;

namespace AetherRemoteServer.Services;

public class StorageService
{
    private static readonly bool EnableVerboseLogging = true;

    private readonly string validSecretsAndFriendCodesPath = Path.Combine("Data", "validSecretsAndFriendCodes.json");
    private readonly Dictionary<string, string> validSecretsAndFriendCodes;

    public StorageService()
    {
        validSecretsAndFriendCodes = LoadSecretsAndFriendCodesFile();
    }

    public bool IsValidSecret(string secret)
    {
        return validSecretsAndFriendCodes.ContainsKey(secret);
    }

    public bool IsValidFriendCode(string friendCode)
    {
        return validSecretsAndFriendCodes.ContainsValue(friendCode);
    }

    public string? TryGetFriendCode(string secret)
    {
        var foundFriendCode = validSecretsAndFriendCodes.TryGetValue(secret, out var friendCode);
        if (EnableVerboseLogging && foundFriendCode == false)
        {
            Log($"Could not find secret in database: {secret}");
        }

        return friendCode;
    }

    private Dictionary<string, string> LoadSecretsAndFriendCodesFile()
    {
        Dictionary<string, string>? result = null;
        try
        {
            if (File.Exists(validSecretsAndFriendCodesPath))
            {
                var raw = File.ReadAllText(validSecretsAndFriendCodesPath);
                result = JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
            }
        }
        catch (Exception ex)
        {
            Log($"Something went wrong loading the secret & friend code file: {ex.Message}");
        }

        result ??= new();
        result.Add("test_account_secret", "test_account_friendcode");
        return result;
    }

    private void Save()
    {
        try
        {
            var raw = JsonSerializer.Serialize(validSecretsAndFriendCodes);
            File.WriteAllText(validSecretsAndFriendCodesPath, raw);
        }
        catch(Exception ex)
        {
            Log($"Error saving database: {ex.Message}");
        }
    }

    private static void Log(string message)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[StorageService] ");
        sb.AppendLine(message);
        Console.WriteLine(sb.ToString());
    }
}
