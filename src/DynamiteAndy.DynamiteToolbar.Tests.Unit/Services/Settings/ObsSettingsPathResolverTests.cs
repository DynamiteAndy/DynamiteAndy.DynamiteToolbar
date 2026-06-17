using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services.Settings;

[TestFixture]
public class ObsSettingsPathResolverTests
{
    private Mock<IObsApi> _obsApiMock = null!;
    private Mock<IFileService> _fileMock = null!;
    private Mock<ILogger> _loggerMock = null!;

    private ObsSettingsPathResolver CreateSut(string fileName = "settings.json")
        => new(fileName, _obsApiMock.Object, _fileMock.Object, _loggerMock.Object);

    [SetUp]
    public void SetUp()
    {
        _obsApiMock = new Mock<IObsApi>();
        _fileMock = new Mock<IFileService>();
        _loggerMock = new Mock<ILogger>();
    }

    // -----------------------------------------------------------------------
    // Happy path
    // -----------------------------------------------------------------------

    [Test]
    public void GetSettingsPath_ReturnsPathFromObsApi()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath("settings.json"))
                   .Returns("/obs/config/settings.json");

        //Act
        var result = CreateSut().GetSettingsPath();

        //Assert
        Assert.That(result, Is.EqualTo("/obs/config/settings.json"));
    }

    [Test]
    public void GetSettingsPath_CreatesDirectory_WhenDirectoryPartExists()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns("/obs/config/plugin/settings.json");

        //Act
        CreateSut().GetSettingsPath();

        //Assert — use Path.GetDirectoryName so the expected value matches the OS separator
        var expected = Path.GetDirectoryName("/obs/config/plugin/settings.json");
        _fileMock.Verify(f => f.CreateDirectory(expected!), Times.Once);
    }

    [Test]
    public void GetSettingsPath_DoesNotCreateDirectory_WhenPathHasNoDirectory()
    {
        //Arrange — Path.GetDirectoryName("settings.json") returns "" on some runtimes
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns("settings.json");

        //Act
        CreateSut().GetSettingsPath();

        //Assert
        _fileMock.Verify(f => f.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetSettingsPath_PassesFileNameToObsApi()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath("myfile.json"))
                   .Returns("/some/path/myfile.json");

        //Act
        CreateSut("myfile.json").GetSettingsPath();

        //Assert
        _obsApiMock.Verify(a => a.ModuleGetConfigPath("myfile.json"), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Exception handling
    // -----------------------------------------------------------------------

    [Test]
    public void GetSettingsPath_WhenObsApiThrows_ReturnsNull()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Throws(new Exception("OBS error"));

        //Act
        var result = CreateSut().GetSettingsPath();

        //Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetSettingsPath_WhenObsApiThrows_LogsError()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Throws(new Exception("OBS error"));

        //Act
        CreateSut().GetSettingsPath();

        //Assert
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("OBS error"))), Times.Once);
    }

    [Test]
    public void GetSettingsPath_WhenCreateDirectoryThrows_ReturnsNull()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns("/obs/config/plugin/settings.json");
        _fileMock.Setup(f => f.CreateDirectory(It.IsAny<string>()))
                 .Throws(new IOException("Disk full"));

        //Act
        var result = CreateSut().GetSettingsPath();

        //Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetSettingsPath_WhenCreateDirectoryThrows_LogsError()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns("/obs/config/plugin/settings.json");
        _fileMock.Setup(f => f.CreateDirectory(It.IsAny<string>()))
                 .Throws(new IOException("Disk full"));

        //Act
        CreateSut().GetSettingsPath();

        //Assert
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("Disk full"))), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Null / empty return from OBS API
    // -----------------------------------------------------------------------

    [Test]
    public void GetSettingsPath_WhenObsApiReturnsNull_ReturnsNull()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns((string?)null);

        //Act
        var result = CreateSut().GetSettingsPath();

        //Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetSettingsPath_WhenObsApiReturnsNull_DoesNotCreateDirectory()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleGetConfigPath(It.IsAny<string>()))
                   .Returns((string?)null);

        //Act
        CreateSut().GetSettingsPath();

        //Assert
        _fileMock.Verify(f => f.CreateDirectory(It.IsAny<string>()), Times.Never);
    }
}
