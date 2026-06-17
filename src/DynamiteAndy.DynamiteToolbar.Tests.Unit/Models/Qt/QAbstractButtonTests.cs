using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models.Qt;

[TestFixture]
public class QAbstractButtonTests
{
    private Mock<IQAbstractButtonShim> _btnMock = null!;
    private Mock<IQWidgetShim> _widgetMock = null!;
    private static readonly IntPtr Handle = new(0x200);

    [SetUp]
    public void SetUp()
    {
        _btnMock = new Mock<IQAbstractButtonShim>();
        _widgetMock = new Mock<IQWidgetShim>();
    }

    private QAbstractButton Create() => new(Handle, _btnMock.Object, _widgetMock.Object);

    // -----------------------------------------------------------------------
    // Text
    // -----------------------------------------------------------------------

    [Test]
    public void SetText_CallsShim()
    {
        //Arrange
        var button = Create();

        //Act
        button.Text = "Click me";

        //Assert
        _btnMock.Verify(s => s.SetText(Handle, "Click me"), Times.Once);
    }

    [Test]
    public void SetText_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var button = Create();

        //Act
        button.Text = "same";
        button.Text = "same";

        //Assert
        _btnMock.Verify(s => s.SetText(Handle, "same"), Times.Once);
    }

    [Test]
    public void SetText_Null_DoesNotCallShim()
    {
        //Arrange
        var button = Create();

        //Act
        button.Text = null;

        //Assert
        _btnMock.Verify(s => s.SetText(It.IsAny<IntPtr>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void SetText_ChangedValue_CallsShimAgain()
    {
        //Arrange
        var button = Create();

        //Act
        button.Text = "first";
        button.Text = "second";

        //Assert
        _btnMock.Verify(s => s.SetText(Handle, "first"), Times.Once);
        _btnMock.Verify(s => s.SetText(Handle, "second"), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Checked
    // -----------------------------------------------------------------------

    [Test]
    public void SetChecked_True_CallsShim()
    {
        //Arrange
        var button = Create();

        //Act
        button.Checked = true;

        //Assert
        _btnMock.Verify(s => s.SetChecked(Handle, true), Times.Once);
    }

    [Test]
    public void SetChecked_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var button = Create();

        //Act
        button.Checked = true;
        button.Checked = true;

        //Assert
        _btnMock.Verify(s => s.SetChecked(Handle, true), Times.Once);
    }

    [Test]
    public void SetChecked_False_WhenAlreadyFalse_DoesNotCallShim()
    {
        //Arrange
        var button = Create();

        //Act
        button.Checked = false;

        //Assert
        _btnMock.Verify(s => s.SetChecked(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // RegisterCallback
    // -----------------------------------------------------------------------

    [Test]
    public void RegisterCallback_CallsOnClickedWithNonZeroPtr()
    {
        //Arrange
        var button = Create();

        //Act
        button.RegisterCallback();

        //Assert
        _btnMock.Verify(s => s.OnClicked(Handle, It.Is<IntPtr>(p => p != IntPtr.Zero)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_ClearsCallback()
    {
        //Arrange
        var button = Create();

        //Act
        button.RegisterCallback();
        button.Dispose();

        //Assert
        _btnMock.Verify(s => s.OnClicked(Handle, IntPtr.Zero), Times.Once);
    }

    [Test]
    public void Dispose_CalledTwice_ClearsCallbackOnlyOnce()
    {
        //Arrange
        var button = Create();

        //Act
        button.RegisterCallback();
        button.Dispose();
        button.Dispose();

        //Assert
        _btnMock.Verify(s => s.OnClicked(Handle, IntPtr.Zero), Times.Once);
    }
}
