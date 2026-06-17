using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Services;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Services;

[TestFixture]
public class ObsLocalizationServiceTests
{
    private Mock<IObsApi> _obsApiMock = null!;
    private Mock<IObsTextLookupApi> _obsTextLookupApiMock = null!;
    private ObsLocalizationService _sut = null!;

    private static readonly IntPtr FakeLookupHandle = new(0xDEAD);

    [SetUp]
    public void SetUp()
    {
        _obsApiMock = new Mock<IObsApi>();
        _obsTextLookupApiMock = new Mock<IObsTextLookupApi>();
        _sut = new ObsLocalizationService(_obsApiMock.Object, _obsTextLookupApiMock.Object);
    }

    [TearDown]
    public void TearDown() => _sut.Dispose();

    // -----------------------------------------------------------------------
    // DefaultLocale
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultLocale_IsEnGb()
    {
        //Arrange + Act + Assert
        Assert.That(_sut.DefaultLocale, Is.EqualTo("en-GB"));
    }

    // -----------------------------------------------------------------------
    // T — before SetLocale
    // -----------------------------------------------------------------------

    [Test]
    public void Translate_BeforeSetLocale_ReturnsIdentifier()
    {
        //Arrange + Act
        var result = _sut.T("some.key");

        //Assert
        Assert.That(result, Is.EqualTo("some.key"));
    }

    // -----------------------------------------------------------------------
    // SetLocale
    // -----------------------------------------------------------------------

    [Test]
    public void SetLocale_CallsModuleLoadLocale_WithDefaultAndSuppliedLocale()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);

        //Act
        _sut.SetLocale("fr-FR");

        //Assert
        _obsApiMock.Verify(a => a.ModuleLoadLocale("en-GB", "fr-FR"), Times.Once);
    }

    [Test]
    public void SetLocale_CalledTwice_DestroysPreviousLookup()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);

        //Act
        _sut.SetLocale("fr-FR");
        _sut.SetLocale("de-DE");

        //Assert
        _obsTextLookupApiMock.Verify(t => t.Destroy(FakeLookupHandle), Times.Once);
    }

    [Test]
    public void SetLocale_FirstCall_DoesNotCallDestroyBefore()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);

        //Act
        _sut.SetLocale("fr-FR");

        //Assert
        _obsTextLookupApiMock.Verify(t => t.Destroy(It.IsAny<IntPtr>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // T — after SetLocale
    // -----------------------------------------------------------------------

    [Test]
    public void Translate_AfterSetLocale_ReturnsLookupResult()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);
        _obsTextLookupApiMock.Setup(t => t.GetString(FakeLookupHandle, "hello.key"))
                             .Returns("Bonjour");
        _sut.SetLocale("fr-FR");

        //Act
        var result = _sut.T("hello.key");

        //Assert
        Assert.That(result, Is.EqualTo("Bonjour"));
    }

    [Test]
    public void Translate_LookupReturnsNull_FallsBackToIdentifier()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);
        _obsTextLookupApiMock.Setup(t => t.GetString(FakeLookupHandle, It.IsAny<string>()))
                             .Returns((string?)null);
        _sut.SetLocale("fr-FR");

        //Act
        var result = _sut.T("missing.key");

        //Assert
        Assert.That(result, Is.EqualTo("missing.key"));
    }

    [Test]
    public void Translate_LoadLocaleReturnsZero_ReturnsIdentifier()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(IntPtr.Zero);
        _sut.SetLocale("fr-FR");

        //Act
        var result = _sut.T("any.key");

        //Assert
        Assert.That(result, Is.EqualTo("any.key"));
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_WithActiveLookup_CallsDestroy()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);
        _sut.SetLocale("fr-FR");

        //Act
        _sut.Dispose();

        //Assert
        _obsTextLookupApiMock.Verify(t => t.Destroy(FakeLookupHandle), Times.Once);
    }

    [Test]
    public void Dispose_WithoutSetLocale_DoesNotCallDestroy()
    {
        //Arrange + Act
        _sut.Dispose();

        //Assert
        _obsTextLookupApiMock.Verify(t => t.Destroy(It.IsAny<IntPtr>()), Times.Never);
    }

    [Test]
    public void Dispose_CalledTwice_CallsDestroyOnlyOnce()
    {
        //Arrange
        _obsApiMock.Setup(a => a.ModuleLoadLocale(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(FakeLookupHandle);
        _sut.SetLocale("fr-FR");

        //Act
        _sut.Dispose();
        _sut.Dispose();

        //Assert
        _obsTextLookupApiMock.Verify(t => t.Destroy(FakeLookupHandle), Times.Once);
    }
}
