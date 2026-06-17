using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Interop.Bridge;
using DynamiteAndy.DynamiteToolbar.Services.Settings;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dialog;
using DynamiteAndy.DynamiteToolbar.UI.Menu;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Menu;

[TestFixture]
public class ToolsMenuTests
{
    private Mock<IObsFrontendApi> _frontendMock = null!;
    private Mock<ILocalizationService> _localeMock = null!;
    private Mock<IQActionShim> _actionShimMock = null!;

    private static readonly IntPtr ActionPtr = new(0x999);

    [SetUp]
    public void SetUp()
    {
        _frontendMock = new Mock<IObsFrontendApi>();
        _localeMock = new Mock<ILocalizationService>();
        _actionShimMock = new Mock<IQActionShim>();

        _localeMock.Setup(l => l.T(It.IsAny<string>())).Returns<string>(k => k);
        _frontendMock.Setup(f => f.AddToolsMenuQAction(It.IsAny<string>())).Returns(ActionPtr);
    }

    private ToolsMenu CreateSut() => new(
        _frontendMock.Object, _localeMock.Object,
        _actionShimMock.Object,
        // SettingsDialog can't be mocked without an interface; use a relaxed constructor
        // by instantiating with all-null mocks — Register() wires callbacks only.
        new SettingsDialog(
            new Mock<ISettingsDialogShim>().Object,
            new Mock<ISettingsService>().Object,
            new Mock<ILocalizationService>().Object,
            new Mock<IQGroupBoxShim>().Object,
            new Mock<IQAbstractButtonShim>().Object,
            new Mock<IQLabelShim>().Object,
            new Mock<IPubSubService>().Object));

    // -----------------------------------------------------------------------
    // Register
    // -----------------------------------------------------------------------

    [Test]
    public void Register_AddsToolsMenuAction()
    {
        //Arrange
        var menu = CreateSut();

        //Act
        menu.Register();

        //Assert
        _frontendMock.Verify(f => f.AddToolsMenuQAction(
            "Toolbar.Menu.Tools.Settings"), Times.Once);
    }

    [Test]
    public void Register_WiresCallbackOnAction()
    {
        //Arrange
        var menu = CreateSut();

        //Act
        menu.Register();

        //Assert
        _actionShimMock.Verify(
            a => a.OnTriggered(ActionPtr, It.Is<IntPtr>(p => p != IntPtr.Zero)),
            Times.Once);
    }

    [Test]
    public void Register_CalledTwice_AddsActionTwice()
    {
        //Arrange
        var menu = CreateSut();

        //Act
        menu.Register();
        menu.Register();

        //Assert
        _frontendMock.Verify(f => f.AddToolsMenuQAction(It.IsAny<string>()), Times.Exactly(2));
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_ClearsCallback()
    {
        //Arrange
        var menu = CreateSut();
        menu.Register();

        //Act
        menu.Dispose();

        //Assert
        _actionShimMock.Verify(a => a.OnTriggered(ActionPtr, IntPtr.Zero), Times.Once);
    }

    [Test]
    public void Dispose_WithoutRegister_DoesNotThrow()
    {
        //Arrange
        var menu = CreateSut();

        //Act + Assert
        Assert.DoesNotThrow(() => menu.Dispose());
    }

    [Test]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        //Arrange
        var menu = CreateSut();
        menu.Register();

        //Act + Assert
        Assert.DoesNotThrow(() => { menu.Dispose(); menu.Dispose(); });
    }

    [Test]
    public void Dispose_CalledTwice_ClearsCallbackOnlyOnce()
    {
        //Arrange
        var menu = CreateSut();
        menu.Register();

        //Act
        menu.Dispose();
        menu.Dispose();

        //Assert
        _actionShimMock.Verify(a => a.OnTriggered(ActionPtr, IntPtr.Zero), Times.Once);
    }
}
