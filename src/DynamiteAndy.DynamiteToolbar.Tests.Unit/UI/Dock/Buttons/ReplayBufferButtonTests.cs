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
public class ReplayBufferButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.ReplayBuffer";
    protected override IntPtr ExpectedButtonPtr => ReplayBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    [Test]
    public override void Constructor_SetsText()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert — button uses tooltip not text
        WidgetMock.Verify(s => s.SetTooltip(ReplayBtnPtr, "Toolbar.Button.ReplayBuffer.Start"), Times.Once);
    }

    private ReplayBufferButton Create(
        bool replayActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.ReplayBufferActive).Returns(replayActive);

        return CreateButton(
            () => new ReplayBufferButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            visible, active, clearInvocations);
    }

    private ReplayBufferButtonTestable CreateTestable(
        bool replayActive = false,
        bool visible = true,
        bool active = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.ReplayBufferActive).Returns(replayActive);

        return CreateButton(
            () => new ReplayBufferButtonTestable(
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
        _ = Create(replayActive: false, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(ReplayBtnPtr, false), Times.Never);
    }

    [Test]
    public void Constructor_WhenActive_IsChecked()
    {
        //Arrange + Act
        _ = Create(replayActive: true, clearInvocations: false);

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(ReplayBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_ReplayBufferStarted_ChecksButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_REPLAY_BUFFER_STARTED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(ReplayBtnPtr, true), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_ReplayBufferStopped_UnchecksButton()
    {
        //Arrange — active: true reflects replay state; CreateButton clears invocations
        var button = Create(active: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_REPLAY_BUFFER_STOPPED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(ReplayBtnPtr, false), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_SameState_DoesNotUpdateShim()
    {
        //Arrange — active: true reflects replay state; CreateButton clears invocations
        var button = Create(active: true);

        //Act — same state as current
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_REPLAY_BUFFER_STARTED));

        //Assert — no redundant shim calls when state hasn't changed
        BtnShimMock.Verify(s => s.SetChecked(ReplayBtnPtr, It.IsAny<bool>()), Times.Never);
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
    public void OnSettingsChangedEvent_HideReplayBuffer_HidesButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideReplayBuffer = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(ReplayBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowReplayBuffer_ShowsButton()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var button = Create(visible: false);

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideReplayBuffer = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(ReplayBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked — toggles replay buffer
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenInactive_StartsReplayBuffer()
    {
        //Arrange
        var button = CreateTestable(replayActive: false);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.ReplayBufferStart(), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_StopsReplayBuffer()
    {
        //Arrange
        var button = CreateTestable(replayActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.ReplayBufferStop(), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_SavesReplayBeforeStopping()
    {
        //Arrange
        var button = CreateTestable(replayActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.ReplayBufferSave(), Times.Once);
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

    private sealed class ReplayBufferButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IObsFrontendApi frontend,
        IPubSubService pubSub,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : ReplayBufferButton(ctx, locale, frontend, pubSub, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
