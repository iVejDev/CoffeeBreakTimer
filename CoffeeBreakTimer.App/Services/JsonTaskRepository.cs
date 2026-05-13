using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Interfaces;
using System.Text.Json;

namespace CoffeeBreakTimer.App.Services;

public sealed class JsonTaskRepository : ITaskRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonTaskRepository()
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "workspace");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "tasks.json");
    }

    public async Task<IReadOnlyList<FocusTask>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var tasks = await JsonSerializer.DeserializeAsync<List<FocusTask>>(
                stream,
                SerializerOptions,
                cancellationToken);

            return tasks?.Where(IsValidTask).ToList() ?? [];
        }
        catch (JsonException)
        {
            BackupUnreadableFile();
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    public async Task SaveAsync(IReadOnlyCollection<FocusTask> tasks, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var sanitizedTasks = tasks
            .Where(IsValidTask)
            .OrderBy(task => task.IsCompleted)
            .ThenByDescending(task => task.CreatedAt)
            .ToList();

        var tempPath = $"{_filePath}.tmp";

        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, sanitizedTasks, SerializerOptions, cancellationToken);
        }

        File.Move(tempPath, _filePath, overwrite: true);
    }

    private void BackupUnreadableFile()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        var backupPath = $"{_filePath}.{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.bak";
        File.Move(_filePath, backupPath, overwrite: true);
    }

    private static bool IsValidTask(FocusTask task)
    {
        return task.Id != Guid.Empty && !string.IsNullOrWhiteSpace(task.Title);
    }
}
