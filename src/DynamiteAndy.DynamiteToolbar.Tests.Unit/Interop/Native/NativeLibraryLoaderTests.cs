using DynamiteAndy.DynamiteToolbar.Interop.Native;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.Services.IO;
using Moq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Interop.Native;

[TestFixture]
public class NativeLibraryLoaderTests
{
    private Mock<ILogger> _loggerMock = null!;
    private Mock<IFileService> _fileMock = null!;
    private Mock<IObsApi> _obsApiMock = null!;
    private Mock<IRuntimeInformationProxy> _runtimeMock = null!;
    private Mock<INativeLibraryProxy> _nativeLibMock = null!;

    private static readonly IntPtr FakeHandle = new(0xABCD);
    private const string LibName = "TestLib";

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger>();
        _fileMock = new Mock<IFileService>();
        _obsApiMock = new Mock<IObsApi>();
        _runtimeMock = new Mock<IRuntimeInformationProxy>();
        _nativeLibMock = new Mock<INativeLibraryProxy>();

        // Default: Windows, file exists, load succeeds
        _runtimeMock.Setup(r => r.IsWindows()).Returns(true);
        _runtimeMock.Setup(r => r.IsMacOS()).Returns(false);
        _fileMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
        _nativeLibMock.Setup(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()))
                      .Returns(FakeHandle);
    }

    private NativeLibraryLoader CreateSut() =>
        new(LibName, _loggerMock.Object, _fileMock.Object,
            _obsApiMock.Object, _runtimeMock.Object, _nativeLibMock.Object);

    // -----------------------------------------------------------------------
    // LibraryName
    // -----------------------------------------------------------------------

    [Test]
    public void LibraryName_ReturnsConstructorValue()
    {
        //Arrange + Act
        var sut = CreateSut();

        //Assert
        Assert.That(sut.LibraryName, Is.EqualTo(LibName));
    }

    // -----------------------------------------------------------------------
    // IsLoaded — initial state
    // -----------------------------------------------------------------------

    [Test]
    public void IsLoaded_BeforeLoad_IsFalse()
    {
        //Arrange + Act
        var sut = CreateSut();

        //Assert
        Assert.That(sut.IsLoaded, Is.False);
    }

    // -----------------------------------------------------------------------
    // Load — with explicit override path
    // -----------------------------------------------------------------------

    [Test]
    public void Load_WithOverridePath_FileExists_CallsNativeLoad()
    {
        //Arrange
        var sut = CreateSut();

        //Act
        sut.Load("/override/TestLib.dll");

        //Assert
        _nativeLibMock.Verify(n => n.Load("/override/TestLib.dll", It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Once);
    }

    [Test]
    public void Load_WithOverridePath_FileExists_IsLoadedBecomesTrue()
    {
        //Arrange
        var sut = CreateSut();

        //Act
        sut.Load("/override/TestLib.dll");

        //Assert
        Assert.That(sut.IsLoaded, Is.True);
    }

    [Test]
    public void Load_WithOverridePath_FileExists_LogsSuccess()
    {
        //Arrange
        var sut = CreateSut();

        //Act
        sut.Load("/override/TestLib.dll");

        //Assert
        _loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("Loaded successfully"))), Times.Once);
    }

    [Test]
    public void Load_WithOverridePath_FileNotFound_DoesNotCallNativeLoad()
    {
        //Arrange
        _fileMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);
        var sut = CreateSut();

        //Act
        sut.Load("/missing/TestLib.dll");

        //Assert
        _nativeLibMock.Verify(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Never);
    }

    [Test]
    public void Load_WithOverridePath_FileNotFound_LogsError()
    {
        //Arrange
        _fileMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);
        var sut = CreateSut();

        //Act
        sut.Load("/missing/TestLib.dll");

        //Assert
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("File not found"))), Times.Once);
    }

    [Test]
    public void Load_WhenAlreadyLoaded_DoesNotCallNativeLoadAgain()
    {
        //Arrange
        var sut = CreateSut();
        sut.Load("/override/TestLib.dll");
        _nativeLibMock.Invocations.Clear();

        //Act
        sut.Load("/override/TestLib.dll");

        //Assert
        _nativeLibMock.Verify(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Never);
    }

    [Test]
    public void Load_WhenNativeLoadThrows_LogsError()
    {
        //Arrange
        _nativeLibMock.Setup(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()))
                      .Throws(new Exception("load failed"));
        var sut = CreateSut();

        //Act + Assert
        Assert.DoesNotThrow(() => sut.Load("/any/path.dll"));
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("load failed"))), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Load — default path resolution (no override)
    // -----------------------------------------------------------------------

    [Test]
    public void Load_NoOverride_Windows_AppendsDllExtension()
    {
        //Arrange
        _runtimeMock.Setup(r => r.IsWindows()).Returns(true);
        _obsApiMock.Setup(a => a.GetModuleBinaryPath()).Returns("/obs/bin/module.dll");
        var sut = CreateSut();

        //Act
        sut.Load();

        //Assert — resolved path should end with TestLib.dll
        _nativeLibMock.Verify(n => n.Load(
            It.Is<string>(p => p.EndsWith($"{LibName}.dll")),
            It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Once);
    }

    [Test]
    public void Load_NoOverride_MacOS_AppendsDylibExtension()
    {
        //Arrange
        _runtimeMock.Setup(r => r.IsWindows()).Returns(false);
        _runtimeMock.Setup(r => r.IsMacOS()).Returns(true);
        _obsApiMock.Setup(a => a.GetModuleBinaryPath()).Returns("/obs/bin/module.dylib");
        var sut = CreateSut();

        //Act
        sut.Load();

        //Assert
        _nativeLibMock.Verify(n => n.Load(
            It.Is<string>(p => p.EndsWith($"{LibName}.dylib")),
            It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Once);
    }

    [Test]
    public void Load_NoOverride_Linux_AppendsSoExtension()
    {
        //Arrange
        _runtimeMock.Setup(r => r.IsWindows()).Returns(false);
        _runtimeMock.Setup(r => r.IsMacOS()).Returns(false);
        _obsApiMock.Setup(a => a.GetModuleBinaryPath()).Returns("/obs/bin/module.so");
        var sut = CreateSut();

        //Act
        sut.Load();

        //Assert
        _nativeLibMock.Verify(n => n.Load(
            It.Is<string>(p => p.EndsWith($"{LibName}.so")),
            It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Once);
    }

    [Test]
    public void Load_NoOverride_NullBinaryPath_LogsError()
    {
        //Arrange
        _obsApiMock.Setup(a => a.GetModuleBinaryPath()).Returns((string?)null);
        var sut = CreateSut();

        //Act
        sut.Load();

        //Assert
        _nativeLibMock.Verify(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Never);
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("File not found"))), Times.Once);
    }

    [Test]
    public void Load_NoOverride_EmptyBinaryPath_LogsError()
    {
        //Arrange
        _obsApiMock.Setup(a => a.GetModuleBinaryPath()).Returns(string.Empty);
        var sut = CreateSut();

        //Act
        sut.Load();

        //Assert
        _nativeLibMock.Verify(n => n.Load(It.IsAny<string>(), It.IsAny<Assembly>(), It.IsAny<DllImportSearchPath?>()), Times.Never);
        _loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains("File not found"))), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_WhenLoaded_CallsFree()
    {
        //Arrange
        var sut = CreateSut();
        sut.Load("/override/TestLib.dll");

        //Act
        sut.Dispose();

        //Assert
        _nativeLibMock.Verify(n => n.Free(FakeHandle), Times.Once);
    }

    [Test]
    public void Dispose_WhenLoaded_IsLoadedBecomesFalse()
    {
        //Arrange
        var sut = CreateSut();
        sut.Load("/override/TestLib.dll");

        //Act
        sut.Dispose();

        //Assert
        Assert.That(sut.IsLoaded, Is.False);
    }

    [Test]
    public void Dispose_WhenNotLoaded_DoesNotCallFree()
    {
        //Arrange
        var sut = CreateSut();

        //Act
        sut.Dispose();

        //Assert
        _nativeLibMock.Verify(n => n.Free(It.IsAny<IntPtr>()), Times.Never);
    }

    [Test]
    public void Dispose_CalledTwice_CallsFreeOnlyOnce()
    {
        //Arrange
        var sut = CreateSut();
        sut.Load("/override/TestLib.dll");

        //Act
        sut.Dispose();
        sut.Dispose();

        //Assert
        _nativeLibMock.Verify(n => n.Free(It.IsAny<IntPtr>()), Times.Once);
    }
}
