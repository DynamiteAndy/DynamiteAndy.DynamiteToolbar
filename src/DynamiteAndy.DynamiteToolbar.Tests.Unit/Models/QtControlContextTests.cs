using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Shim;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models;

[TestFixture]
public class QtControlContextTests
{
    [Test]
    public void Dock_DefaultsToNull()
    {
        //Arrange + Act
        var ctx = new QtControlContext();

        //Assert
        Assert.That(ctx.Dock, Is.Null);
    }

    [Test]
    public void SetQtControlContext_SetsDock()
    {
        //Arrange
        var ctx = new QtControlContext();
        var dock = new PluginDock { DockPtr = new IntPtr(0x42) };

        //Act
        ctx.SetQtControlContext(dock);

        //Assert
        Assert.That(ctx.Dock, Is.Not.Null);
        Assert.That(ctx.Dock!.Value.DockPtr, Is.EqualTo(new IntPtr(0x42)));
    }

    [Test]
    public void SetQtControlContext_CalledTwice_ThrowsInvalidOperationException()
    {
        //Arrange
        var ctx = new QtControlContext();
        var dock = new PluginDock();
        ctx.SetQtControlContext(dock);

        //Act + Assert
        Assert.Throws<InvalidOperationException>(() => ctx.SetQtControlContext(dock));
    }
}
