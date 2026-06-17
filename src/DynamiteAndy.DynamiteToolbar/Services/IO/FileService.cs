namespace DynamiteAndy.DynamiteToolbar.Services.IO;

public class FileService : IFileService
{
    public bool FileExists(string path) => File.Exists(path);

    public void CreateDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}