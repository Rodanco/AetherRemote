using AetherRemoteServer.Domain;
using System.Text;
using System.Text.Json;

namespace AetherRemoteServer.Services;

public class StorageService
{
    private static readonly string UserDataFileName = "userData.json";
    private static readonly string UserDataPartialPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
    private static readonly JsonSerializerOptions SaveOptions = new() { WriteIndented = true };

    private readonly Dictionary<string, UserData> userDataDictionary;

    /// <summary>
    /// There are situations where the server may fail to load the 'database'. In that case, it will
    /// return an empty list. If a valid file exists, but it just fails to load, a save would write over
    /// the contents of the valid file, which is bad. This variable will be set to true only if the original
    /// file fails to load, and when it is set to true, it instructs all saving to be done to an alternate file
    /// </summary>
    private bool safeguard = false;

    public StorageService()
    {
        userDataDictionary = LoadUserData();
    }

    public UserData? TryGetUserData(string secret)
    {
        if (userDataDictionary.TryGetValue(secret, out var userData))
        {
            return userData;
        }

        return null;
    }

    private Dictionary<string, UserData> LoadUserData()
    {
        Dictionary<string, UserData>? result = null;
        try
        {
            var path = Path.Combine(UserDataPartialPath, UserDataFileName);
            if (File.Exists(path))
            {
                var serializedUserData = File.ReadAllText(path);
                result = JsonSerializer.Deserialize<Dictionary<string, UserData>>(serializedUserData);
            }
            else
            {
                File.Create(path).Dispose();
                result = new();
            }
        }
        catch (Exception ex)
        {
            Log($"Error loading database: {ex.Message}", true);
        }

        // Something went wrong, fallback to safeguard
        if (result == null)
        {
            safeguard = true;
            result = new();
        }
            
        return result;
    }

    private void SaveUserData()
    {
        try
        {
            var filename = safeguard ? $"safe_{UserDataFileName}" : UserDataFileName;
            var path = Path.Combine(UserDataPartialPath, filename);
            var serializedUserData = JsonSerializer.Serialize(userDataDictionary, SaveOptions);
            File.WriteAllText(path, serializedUserData);
        }
        catch(Exception ex)
        {
            Log($"Error saving database: {ex.Message}");
        }
    }

    private static void Log(string message, bool critial = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[StorageService] ");
        sb.AppendLine(message);

        if (critial)
            Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(sb.ToString());

        if (critial)
            Console.ResetColor();
    }
}
