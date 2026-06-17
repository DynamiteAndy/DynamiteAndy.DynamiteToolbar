using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.Services;
using Moq;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Buttons;

public abstract class ButtonTestBase
{
    protected Mock<IQAbstractButtonShim> BtnShimMock = null!;
    protected Mock<IQWidgetShim> WidgetMock = null!;
    protected Mock<ILocalizationService> LocaleMock = null!;
    protected Mock<IPubSubService> PubSubMock = null!;
    protected Mock<IObsFrontendApi> FrontendMock = null!;
    protected Mock<IQDockWidgetShim> DockWidgetMock = null!;
    protected Mock<IQMessageBoxShim> MsgBoxMock = null!;
    protected IQtControlContext Context = null!;

    protected static readonly IntPtr FloatBtnPtr = new(0x11);
    protected static readonly IntPtr StudioBtnPtr = new(0x12);
    protected static readonly IntPtr VcamBtnPtr = new(0x13);
    protected static readonly IntPtr ReplayBtnPtr = new(0x14);
    protected static readonly IntPtr RecordBtnPtr = new(0x15);
    protected static readonly IntPtr RecPausedPtr = new(0x16);
    protected static readonly IntPtr StreamBtnPtr = new(0x17);
    protected static readonly IntPtr DockPtr = new(0x18);

    [SetUp]
    public virtual void SetUp()
    {
        BtnShimMock = new Mock<IQAbstractButtonShim>();
        WidgetMock = new Mock<IQWidgetShim>();
        LocaleMock = new Mock<ILocalizationService>();
        PubSubMock = new Mock<IPubSubService>();
        FrontendMock = new Mock<IObsFrontendApi>();
        DockWidgetMock = new Mock<IQDockWidgetShim>();
        MsgBoxMock = new Mock<IQMessageBoxShim>();

        LocaleMock.Setup(l => l.T(It.IsAny<string>())).Returns<string>(k => k);

        var ctx = new QtControlContext();
        ctx.SetQtControlContext(new PluginDock
        {
            DockPtr = DockPtr,
            Buttons = new DockButtons
            {
                FloatBtnPtr = FloatBtnPtr,
                StudioModeBtnPtr = StudioBtnPtr,
                VirtualCameraBtnPtr = VcamBtnPtr,
                ReplayBufferBtnPtr = ReplayBtnPtr,
                RecordBtnPtr = RecordBtnPtr,
                RecordPausedBtnPtr = RecPausedPtr,
                StreamBtnPtr = StreamBtnPtr,
            }
        });
        Context = ctx;
    }

    protected abstract string ExpectedTextKey { get; }
    protected abstract IntPtr ExpectedButtonPtr { get; }
    protected abstract void CreateForTextTest();

    protected T CreateButton<T>(
        Func<T> setup,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true) where T : QAbstractButton
    {
        var button = setup();
        button.Visible = visible;
        button.Checked = active;

        if (clearInvocations)
        {
            BtnShimMock.Invocations.Clear();
            WidgetMock.Invocations.Clear();
        }

        return button;
    }

    [Test]
    public virtual void Constructor_SetsText()
    {
        //Arrange + Act
        CreateForTextTest();

        //Assert
        BtnShimMock.Verify(s => s.SetText(ExpectedButtonPtr, ExpectedTextKey), Times.Once);
    }
}
