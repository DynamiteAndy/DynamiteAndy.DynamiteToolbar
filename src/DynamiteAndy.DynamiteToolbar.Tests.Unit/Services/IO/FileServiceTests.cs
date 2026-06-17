using DynamiteAndy.DynamiteToolbar.Services.IO;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services.IO;

[TestFixture]
public class FileServiceTests
{
    private string _tempDir = null!;
    private FileService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _sut = new FileService();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -----------------------------------------------------------------------
    // FileExists
    // -----------------------------------------------------------------------

    [Test]
    public void FileExists_WhenFilePresent_ReturnsTrue()
    {
        //Arrange
        var path = Path.Combine(_tempDir, "exists.txt");
        File.WriteAllText(path, "data");

        //Act
        var result = _sut.FileExists(path);

        //Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void FileExists_WhenFileMissing_ReturnsFalse()
    {
        //Arrange
        var path = Path.Combine(_tempDir, "missing.txt");

        //Act
        var result = _sut.FileExists(path);

        //Assert
        Assert.That(result, Is.False);
    }

    // -----------------------------------------------------------------------
    // CreateDirectory
    // -----------------------------------------------------------------------

    [Test]
    public void CreateDirectory_WhenParentExists_CreatesDirectory()
    {
        //Arrange — pass the directory path itself (not a file path);
        // the implementation calls Directory.CreateDirectory(path) when
        // Path.GetDirectoryName(path) is non-empty and !Directory.Exists(path)
        var newDir = Path.Combine(_tempDir, "sub");

        //Act
        _sut.CreateDirectory(newDir);

        //Assert
        Assert.That(Directory.Exists(newDir), Is.True);
    }

    [Test]
    public void CreateDirectory_WhenDirectoryAlreadyExists_DoesNotThrow()
    {
        //Arrange — _tempDir already exists; the guard prevents the inner call
        //Act + Assert
        Assert.DoesNotThrow(() => _sut.CreateDirectory(_tempDir));
    }

    [Test]
    public void CreateDirectory_WhenPathHasNoDirectoryComponent_DoesNotThrow()
    {
        //Arrange — Path.GetDirectoryName("filename.json") returns "" on most runtimes;
        // the early-return guard should prevent any Directory call
        //Act + Assert
        Assert.DoesNotThrow(() => _sut.CreateDirectory("filename.json"));
    }

    [Test]
    public void CreateDirectory_WhenPathHasNoDirectoryComponent_CreatesNothing()
    {
        //Arrange
        var before = Directory.GetDirectories(_tempDir);

        //Act
        _sut.CreateDirectory("filename.json");

        //Assert — no new directory was created in the current working dir
        // (we can't check globally, but we verify the path itself wasn't created)
        Assert.That(Directory.Exists("filename.json"), Is.False);
    }
}
