using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models.Qt;

[TestFixture]
public class QLabelTests
{
    private Mock<IQLabelShim> _labelShimMock = null!;
    private Mock<IQWidgetShim> _widgetShimMock = null!;
    private static readonly IntPtr Handle = new(0x500);

    [SetUp]
    public void SetUp()
    {
        _labelShimMock = new Mock<IQLabelShim>();
        _widgetShimMock = new Mock<IQWidgetShim>();
    }

    private QLabel Create() => new(Handle, _labelShimMock.Object, _widgetShimMock.Object);

    // -----------------------------------------------------------------------
    // Default state
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultText_IsNull()
    {
        //Arrange + Act
        var label = Create();

        //Assert
        Assert.That(label.Text, Is.Null);
    }

    // -----------------------------------------------------------------------
    // Text setter
    // -----------------------------------------------------------------------

    [Test]
    public void SetText_CallsShim()
    {
        //Arrange
        var label = Create();

        //Act
        label.Text = "Recording";

        //Assert
        _labelShimMock.Verify(s => s.SetText(Handle, "Recording"), Times.Once);
    }

    [Test]
    public void SetText_SameValueTwice_CallsShimOnlyOnce()
    {
        //Arrange
        var label = Create();

        //Act
        label.Text = "same";
        label.Text = "same";

        //Assert
        _labelShimMock.Verify(s => s.SetText(Handle, "same"), Times.Once);
    }

    [Test]
    public void SetText_Null_DoesNotCallShim()
    {
        //Arrange
        var label = Create();

        //Act
        label.Text = null;

        //Assert
        _labelShimMock.Verify(s => s.SetText(It.IsAny<IntPtr>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void SetText_ChangedValue_CallsShimAgain()
    {
        //Arrange
        var label = Create();

        //Act
        label.Text = "first";
        label.Text = "second";

        //Assert
        _labelShimMock.Verify(s => s.SetText(Handle, "first"), Times.Once);
        _labelShimMock.Verify(s => s.SetText(Handle, "second"), Times.Once);
    }

    [Test]
    public void SetText_AfterNull_CallsShim()
    {
        //Arrange — Text starts null; assigning null again is still null; then a real value
        var label = Create();
        label.Text = null;

        //Act
        label.Text = "now set";

        //Assert
        _labelShimMock.Verify(s => s.SetText(Handle, "now set"), Times.Once);
    }
}
