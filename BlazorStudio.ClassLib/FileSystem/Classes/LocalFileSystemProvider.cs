using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.FileSystem.Classes;

public class LocalFileSystemProvider : IFileSystemProvider
{
    public async Task WriteFileAsync(
        IAbsoluteFilePath absoluteFilePath, 
        string content,
        bool overwrite, 
        bool create,
        CancellationToken cancellationToken = default)
    {
        await File
            .WriteAllTextAsync(
                absoluteFilePath.GetAbsoluteFilePathString(), 
                content, 
                cancellationToken);
    }

    public async Task<string> ReadFileAsync(
        IAbsoluteFilePath absoluteFilePath, 
        CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(
            absoluteFilePath.GetAbsoluteFilePathString(), 
            cancellationToken);
    }

    public Task CreateDirectoryAsync(IAbsoluteFilePath absoluteFilePath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(absoluteFilePath.GetAbsoluteFilePathString());

        return Task.CompletedTask;
    }

    public Task DeleteFileAsync(IAbsoluteFilePath absoluteFilePath, CancellationToken cancellationToken = default)
    {
        File.Delete(
            absoluteFilePath.GetAbsoluteFilePathString());
        
        return Task.CompletedTask;
    }
    
    public Task DeleteDirectoryAsync(IAbsoluteFilePath absoluteFilePath, bool recursive, CancellationToken cancellationToken = default)
    {
        Directory.Delete(
            absoluteFilePath.GetAbsoluteFilePathString(),
            recursive);

        return Task.CompletedTask;
    }
}