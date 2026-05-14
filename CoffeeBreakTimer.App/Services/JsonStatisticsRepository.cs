using CoffeeBreakTimer.Core.Domain;
using CoffeeBreakTimer.Core.Interfaces;
using System.Text.Json;

namespace CoffeeBreakTimer.App.Services;

public sealed class JsonStatisticsRepository : IStatisticsRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonStatisticsRepository()
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "workspace");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "focus-sessions.json");
    }

    public async Task<IReadOnlyList<FocusSessionRecord>> LoadFocusSessionsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var sessions = await JsonSerializer.DeserializeAsync<List<FocusSessionRecord>>(
                stream,
                SerializerOptions,
                cancellationToken);

            return sessions?.Where(IsValidSession).ToList() ?? [];
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

    public async Task SaveFocusSessionsAsync(
        IReadOnlyCollection<FocusSessionRecord> focusSessions,
        CancellationToken cancellationToken = default)
    {
        var sanitizedSessions = focusSessions
            .Where(IsValidSession)
            .OrderByDescending(session => session.CompletedAt)
            .ToList();

        var tempPath = $"{_filePath}.tmp";

        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, sanitizedSessions, SerializerOptions, cancellationToken);
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

    private static bool IsValidSession(FocusSessionRecord session)
    {
        return session.Id != Guid.Empty && session.FocusMinutes > 0;
    }
}
