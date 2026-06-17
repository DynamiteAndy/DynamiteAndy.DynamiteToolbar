using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services.Settings;

[TestFixture]
public class ObsSettingsServiceTests
{
    private Mock<IObsDataApi> _obsDataMock = null!;
    private Mock<ISettingsPathResolver> _resolverMock = null!;
    private Mock<IFileService> _fileMock = null!;
    private ObsSettingsService _sut = null!;

    private const string SettingsPath = "/fake/settings.json";
    private static readonly IntPtr FakeData = new(0xBEEF);

    [SetUp]
    public void SetUp()
    {
        _obsDataMock = new Mock<IObsDataApi>();
        _resolverMock = new Mock<ISettingsPathResolver>();
        _fileMock = new Mock<IFileService>();

        _resolverMock.Setup(r => r.GetSettingsPath()).Returns(SettingsPath);
        _fileMock.Setup(f => f.FileExists(SettingsPath)).Returns(true);
        _obsDataMock.Setup(d => d.LoadFromJson(SettingsPath)).Returns(FakeData);

        _sut = new ObsSettingsService(_obsDataMock.Object, _resolverMock.Object, _fileMock.Object);
    }

    // -----------------------------------------------------------------------
    // Load — happy path
    // -----------------------------------------------------------------------

    [Test]
    public void Load_ReadsAllSettingFields()
    {
        //Arrange
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideFloat))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideStudioMode))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideVirtualCamera))).Returns(false);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideReplayBuffer))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideStartRecording))).Returns(false);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideStartStreaming))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideStreamingLabel))).Returns(false);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideRecordingLabel))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideCurrentSceneLabel))).Returns(false);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideCpuLabel))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideFpsLabel))).Returns(false);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideDroppedFramesLabel))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideKbpsLabel))).Returns(true);
        _obsDataMock.Setup(d => d.GetBool(FakeData, nameof(PluginSettings.HideStopRecordingModal))).Returns(false);

        //Act
        var result = _sut.Load();

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HideFloat, Is.True);
            Assert.That(result.HideStudioMode, Is.True);
            Assert.That(result.HideVirtualCamera, Is.False);
            Assert.That(result.HideReplayBuffer, Is.True);
            Assert.That(result.HideStartRecording, Is.False);
            Assert.That(result.HideStartStreaming, Is.True);
            Assert.That(result.HideStreamingLabel, Is.False);
            Assert.That(result.HideRecordingLabel, Is.True);
            Assert.That(result.HideCurrentSceneLabel, Is.False);
            Assert.That(result.HideCpuLabel, Is.True);
            Assert.That(result.HideFpsLabel, Is.False);
            Assert.That(result.HideDroppedFramesLabel, Is.True);
            Assert.That(result.HideKbpsLabel, Is.True);
            Assert.That(result.HideStopRecordingModal, Is.False);
        }
    }

    [Test]
    public void Load_ReleasesDataHandle_EvenOnSuccess()
    {
        //Arrange + Act
        _sut.Load();

        //Assert
        _obsDataMock.Verify(d => d.Release(FakeData), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Load — edge cases
    // -----------------------------------------------------------------------

    [Test]
    public void Load_PathResolverReturnsNull_ReturnsDefaultSettings()
    {
        //Arrange
        _resolverMock.Setup(r => r.GetSettingsPath()).Returns((string?)null);

        //Act
        var result = _sut.Load();

        //Assert
        Assert.That(result, Is.Not.Null);
        _obsDataMock.Verify(d => d.LoadFromJson(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Load_FileDoesNotExist_ReturnsDefaultSettings()
    {
        //Arrange
        _fileMock.Setup(f => f.FileExists(SettingsPath)).Returns(false);

        //Act
        var result = _sut.Load();

        //Assert
        Assert.That(result, Is.Not.Null);
        _obsDataMock.Verify(d => d.LoadFromJson(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Load_LoadFromJsonReturnsZero_ReturnsDefaultSettings()
    {
        //Arrange
        _obsDataMock.Setup(d => d.LoadFromJson(SettingsPath)).Returns(IntPtr.Zero);

        //Act
        var result = _sut.Load();

        //Assert
        Assert.That(result, Is.Not.Null);
        _obsDataMock.Verify(d => d.Release(It.IsAny<IntPtr>()), Times.Never);
    }

    [Test]
    public void Load_ReleasesDataHandle_WhenGetBoolThrows()
    {
        //Arrange
        _obsDataMock.Setup(d => d.GetBool(FakeData, It.IsAny<string>()))
                    .Throws<InvalidOperationException>();

        //Act + Assert
        Assert.Throws<InvalidOperationException>(() => _sut.Load());
        _obsDataMock.Verify(d => d.Release(FakeData), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Save — happy path
    // -----------------------------------------------------------------------

    [Test]
    public void Save_WritesAllSettingFields()
    {
        //Arrange
        var newData = new IntPtr(0xCAFE);
        _obsDataMock.Setup(d => d.Create()).Returns(newData);

        var settings = new PluginSettings
        {
            HideFloat = true,
            HideStudioMode = false,
            HideVirtualCamera = true,
            HideReplayBuffer = false,
            HideStartRecording = true,
            HideStartStreaming = false,
            HideStreamingLabel = true,
            HideRecordingLabel = false,
            HideCurrentSceneLabel = true,
            HideCpuLabel = false,
            HideFpsLabel = true,
            HideDroppedFramesLabel = false,
            HideKbpsLabel = true,
            HideStopRecordingModal = true,
        };

        //Act
        _sut.Save(settings);

        //Assert
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideFloat), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideStudioMode), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideVirtualCamera), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideReplayBuffer), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideStartRecording), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideStartStreaming), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideStreamingLabel), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideRecordingLabel), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideCurrentSceneLabel), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideCpuLabel), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideFpsLabel), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideDroppedFramesLabel), false), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideKbpsLabel), true), Times.Once);
        _obsDataMock.Verify(d => d.SetBool(newData, nameof(PluginSettings.HideStopRecordingModal), true), Times.Once);
    }

    [Test]
    public void Save_CallsSaveToJson_WithCorrectPathAndHandle()
    {
        //Arrange
        var newData = new IntPtr(0xCAFE);
        _obsDataMock.Setup(d => d.Create()).Returns(newData);

        //Act
        _sut.Save(new PluginSettings());

        //Assert
        _obsDataMock.Verify(d => d.SaveToJson(newData, SettingsPath), Times.Once);
    }

    [Test]
    public void Save_ReleasesDataHandle_AfterSuccess()
    {
        //Arrange
        var newData = new IntPtr(0xCAFE);
        _obsDataMock.Setup(d => d.Create()).Returns(newData);

        //Act
        _sut.Save(new PluginSettings());

        //Assert
        _obsDataMock.Verify(d => d.Release(newData), Times.Once);
    }

    [Test]
    public void Save_ReleasesDataHandle_WhenSetBoolThrows()
    {
        //Arrange
        var newData = new IntPtr(0xCAFE);
        _obsDataMock.Setup(d => d.Create()).Returns(newData);
        _obsDataMock.Setup(d => d.SetBool(newData, It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws<InvalidOperationException>();

        //Act + Assert
        Assert.Throws<InvalidOperationException>(() => _sut.Save(new PluginSettings()));
        _obsDataMock.Verify(d => d.Release(newData), Times.Once);
    }

    [Test]
    public void Save_PathResolverReturnsNull_DoesNotCallCreate()
    {
        //Arrange
        _resolverMock.Setup(r => r.GetSettingsPath()).Returns((string?)null);

        //Act
        _sut.Save(new PluginSettings());

        //Assert
        _obsDataMock.Verify(d => d.Create(), Times.Never);
    }
}
