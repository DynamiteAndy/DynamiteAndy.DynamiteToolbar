using DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;
using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Services;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Buttons;
using Moq;
using ObsInterop;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Buttons;

[TestFixture]
public class VirtualCameraButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.VirtualCamera";
    protected override IntPtr ExpectedButtonPtr => VcamBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    [Test]
    public override void Constructor_SetsText()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert — button uses tooltip not text
        WidgetMock.Verify(s => s.SetTooltip(VcamBtnPtr, "Toolbar.Button.VirtualCamera.Start"), Times.Once);
    }

    private VirtualCameraButton Create(
        bool virtualcamActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.VirtualcamActive).Returns(virtualcamActive);

        return CreateButton(
            () => new VirtualCameraButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible, active, clearInvocations);
    }

    private VirtualCameraButtonTestable CreateTestable(
        bool virtualcamActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.VirtualcamActive).Returns(virtualcamActive);

        return CreateButton(
            () => new VirtualCameraButtonTestable(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible, active, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenInactive_IsUnchecked()
    {
        //Arrange + Act
        _ = Create(virtualcamActive: false, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(VcamBtnPtr, false), Times.Never);
    }

    [Test]
    public void Constructor_WhenActive_IsChecked()
    {
        //Arrange + Act
        _ = Create(virtualcamActive: true, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(VcamBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_VirtualCameraStarted_ChecksButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_VIRTUALCAM_STARTED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(VcamBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_VirtualCameraStopped_UnchecksButton()
    {
        //Arrange — active: true reflects vcam state; CreateButton clears invocations
        var button = Create(active: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_VIRTUALCAM_STOPPED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(VcamBtnPtr, false), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_SameState_DoesNotUpdateShim()
    {
        //Arrange — active: true reflects vcam state; CreateButton clears invocations
        var button = Create(active: true);

        //Act — same state as current
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_VIRTUALCAM_STARTED));

        //Assert — no redundant shim calls when state hasn't changed
        BtnShimMock.Verify(s => s.SetChecked(VcamBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeState()
    {
        //Arrange — CreateButton clears BtnShimMock invocations after construction
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideVirtualCamera_HidesButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideVirtualCamera = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(VcamBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowVirtualCamera_ShowsButton()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var button = Create(visible: false);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideVirtualCamera = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(VcamBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked — toggles virtual camera
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenInactive_StartsVirtualCamera()
    {
        //Arrange
        var button = CreateTestable(virtualcamActive: false);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.StartVirtualcam(), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_StopsVirtualCamera()
    {
        //Arrange
        var button = CreateTestable(virtualcamActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.StopVirtualcam(), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_UnsubscribesFromEvents()
    {
        //Arrange
        var button = Create();

        //Act
        button.Dispose();

        //Assert
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // Testable subclass — exposes OnClicked
    // -----------------------------------------------------------------------

    private sealed class VirtualCameraButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IObsFrontendApi frontend,
        IPubSubService pubSub,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : VirtualCameraButton(ctx, locale, frontend, pubSub, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
