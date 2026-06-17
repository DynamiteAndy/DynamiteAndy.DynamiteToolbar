using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

public abstract class LabelTestBase
{
    protected Mock<IQLabelShim> LabelMock = null!;
    protected Mock<IQWidgetShim> WidgetMock = null!;
    protected Mock<ILocalizationService> LocaleMock = null!;
    protected Mock<IPubSubService> PubSubMock = null!;
    protected Mock<IObsFrontendApi> FrontendMock = null!;
    protected Mock<IObsPlatformApi> PlatformMock = null!;
    protected Mock<IObsApi> ObsApiMock = null!;
    protected IQtControlContext Context = null!;

    protected FakeTimeProvider TimeProvider = null!;

    protected static readonly IntPtr NamePtr = new(0xA1);
    protected static readonly IntPtr DelimPtr = new(0xA2);
    protected static readonly IntPtr ValuePtr = new(0xA3);
    protected static readonly IntPtr SeparatorPtr = new(0xA4);

    [SetUp]
    public virtual void SetUp()
    {
        LabelMock = new Mock<IQLabelShim>();
        WidgetMock = new Mock<IQWidgetShim>();
        LocaleMock = new Mock<ILocalizationService>();
        PubSubMock = new Mock<IPubSubService>();
        FrontendMock = new Mock<IObsFrontendApi>();
        PlatformMock = new Mock<IObsPlatformApi>();
        ObsApiMock = new Mock<IObsApi>();

        TimeProvider = new FakeTimeProvider();

        LocaleMock.Setup(l => l.T(It.IsAny<string>())).Returns<string>(k => k);

        var ctx = new QtControlContext();
        ctx.SetQtControlContext(BuildPluginDock());
        Context = ctx;
    }

    protected abstract PluginDock BuildPluginDock();

    protected abstract string ExpectedNameKey { get; }

    protected abstract void CreateForNameLabelTest();

    protected static DockFrameMonitorLabel MakeDockFrameMonitorLabel() => new()
    {
        NameLblPtr = NamePtr,
        DelimiterLblPtr = DelimPtr,
        ValueLblPtr = ValuePtr,
        SeparatorLblPtr = SeparatorPtr,
    };

    protected T CreateLabel<T>(
        Func<T> setup,
        bool visible = true,
        bool clearInvocations = true) where T : LabelGroup
    {
        var label = setup();
        label.Visible = visible;

        if (clearInvocations)
        {
            LabelMock.Invocations.Clear();
            WidgetMock.Invocations.Clear();
        }

        return label;
    }

    [Test]
    public void Constructor_SetsNameLabel()
    {
        //Arrange + Act
        CreateForNameLabelTest();

        //Assert
        LabelMock.Verify(s => s.SetText(NamePtr, ExpectedNameKey), Times.Once);
    }
}
