namespace DynamiteAndy.DynamiteToolbar.Services.IO;

public interface IFileService
{
    bool FileExists(string path);
    void CreateDirectory(string path);
}