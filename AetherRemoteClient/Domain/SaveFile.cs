using Dalamud.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AetherRemoteClient.Domain;

public class SaveFile<T>
{
    private string filePath { get; init; }

    public T Get => save;
    private readonly T save;

    public SaveFile(string fileDirectory, string fileName)
    {
        filePath = Path.Combine(fileDirectory, fileName);
        save = Load();
    }

    protected T Load()
    {
        T? loaded;
        if (!File.Exists(filePath))
        {
            loaded = (T)Activator.CreateInstance(typeof(T))!;
            Save();
        }
        else
        {
            try
            {
                loaded = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
            }
            catch
            {
                loaded = default;
            }

            if (loaded == null)
            {
                loaded = (T)Activator.CreateInstance(typeof(T))!;
                Save();
            }
        }

        return loaded;
    }

    public void Save()
    {
        Task.Run(SaveAsync);
    }

    public async Task SaveAsync()
    {
        try
        {
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(save, new JsonSerializerOptions() { WriteIndented = true }));
        }
        catch
        {
            // TODO: Logging
        }
    }
}
