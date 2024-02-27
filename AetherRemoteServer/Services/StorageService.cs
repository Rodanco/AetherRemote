using System.Text.Json;

namespace AetherRemoteServer.Services
{
    public class StorageService
    {
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

        public string? GetFriendCode(string secret)
        {
            Console.WriteLine("Trying to get secret " + secret);

            if (!validSecretsAndFriendCodes.TryGetValue(secret, out var friendCode))
            {
                Console.WriteLine($"Secret [{secret}] not found in main file");
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
                Console.WriteLine($"Something went wrong loading the secret & friend code file: {ex.Message}");
            }

            result ??= new();
            result.Add("9921", "jakua97721");
            result.Add("1134", "khada11773");
            result.Add("4659", "venus11832");
            result.Add("3824", "ever11982");
            result.Add("8008", "llarry90202");
            result.Add("7621", "sophie66425");
            result.Add("0082", "kiani82221");
            result.Add("0000", "mora00001");
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
                Console.WriteLine($"Error saving database: {ex.Message}");
            }
        }
    }
}
