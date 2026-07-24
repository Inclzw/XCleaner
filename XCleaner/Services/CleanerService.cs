using System.IO;
using XCleaner.Models;

namespace XCleaner.Services;

public class CleanerService
{
    public Task ScanAsync(string path, IProgress<CleanItem> progress, CancellationToken token = default)
    {
        return Task.Run(() => { ScanDirectory(path, progress, token); }, token);
    }

    private void ScanDirectory(string path, IProgress<CleanItem> progress, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var dir = new DirectoryInfo(path);
            foreach (var subDir in dir.GetDirectories())
            {
                token.ThrowIfCancellationRequested();
                if ((subDir.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    continue;
                }

                var info = CalculateDirectoryInfo(subDir, token);
                var item = new CleanItem
                {
                    Name = subDir.Name,
                    Path = subDir.FullName,
                    Size = info.Size,
                    LastModified = subDir.LastWriteTime,
                    LatestFileModifiedTime = info.LatestFileModifiedTime
                };
                progress.Report(item);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static (long Size, DateTime LatestFileModifiedTime) CalculateDirectoryInfo(DirectoryInfo dir,
        CancellationToken token)
    {
        long size = 0;
        var latest = dir.LastWriteTime;

        try
        {
            foreach (var file in dir.GetFiles())
            {
                token.ThrowIfCancellationRequested();
                size += file.Length;
                if (file.LastWriteTime > latest)
                {
                    latest = file.LastWriteTime;
                }
            }

            foreach (var subDir in dir.GetDirectories())
            {
                token.ThrowIfCancellationRequested();
                if ((subDir.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    continue;
                }

                if (subDir.LastWriteTime > latest)
                {
                    latest = subDir.LastWriteTime;
                }

                var result = CalculateDirectoryInfo(subDir, token);
                size += result.Size;
                if (result.LatestFileModifiedTime > latest)
                {
                    latest = result.LatestFileModifiedTime;
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 没权限的目录跳过
        }

        return (size, latest);
    }
}