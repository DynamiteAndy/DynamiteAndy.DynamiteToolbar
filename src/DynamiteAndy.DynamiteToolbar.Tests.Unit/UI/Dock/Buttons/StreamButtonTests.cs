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
public class StreamButtonTests : ButtonTestBase
{
    protected override string ExpectedTextKey => "Toolbar.Button.Stream.Start";
    protected override IntPtr ExpectedButtonPtr => StreamBtnPtr;
    protected override void CreateForTextTest() => _ = Create(clearInvocations: false);

    // active mirrors streamingActive so CreateButton doesn't desync Checked
    // from what the constructor set. Constructor never calls SetVisible so
    // visible is always left at the QWidget default (true).
    private StreamButton Create(
        bool streamingActive = false,
        bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.StreamingActive).Returns(streamingActive);

        return CreateButton(
            () => new StreamButton(
                Context, LocaleMock.Object, FrontendMock.Object,
                MsgBoxMock.Object, PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            active: streamingActive,
            clearInvocations: clearInvocations);
    }

    private StreamButtonTestable CreateTestable(bool streamingActive = false)
    {
        FrontendMock.Setup(f => f.StreamingActive).Returns(streamingActive);

        return CreateButton(
            () => new StreamButtonTestable(
                Context, LocaleMock.Object, FrontendMock.Object,
                MsgBoxMock.Object, PubSubMock.Object, BtnShimMock.Object, WidgetMock.Object),
            active: streamingActive);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenIdle_SetsStartText()
    {
        //Arrange + Act
        _ = Create(clearInvocations: false);

        //Assert — Text fires SetText (null→value); Checked defaults false so guard blocks;
        //         constructor never calls SetVisible
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, "Toolbar.Button.Stream.Start"), Times.Once);
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, It.IsAny<bool>()), Times.Never);
        WidgetMock.Verify(s => s.SetVisible(StreamBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Constructor_WhenActive_SetsStopTextAndChecked()
    {
        //Arrange + Act
        _ = Create(streamingActive: true, clearInvocations: false);

        //Assert — Checked flips false→true so SetChecked fires; Text fires; Visible never set
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, "Toolbar.Button.Stream.Stop"), Times.Once);
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, true), Times.Once);
        WidgetMock.Verify(s => s.SetVisible(StreamBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — state transitions
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_StreamingStarting_ChecksAndSetsStopText()
    {
        //Arrange — idle: Checked=false
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING));

        //Assert — isActive(true) != Checked(false) → fires
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, true), Times.Once);
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, "Toolbar.Button.Stream.Stop"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStarted_ChecksAndSetsStopText()
    {
        //Arrange — idle: Checked=false
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert — isActive(true) != Checked(false) → fires
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, true), Times.Once);
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, "Toolbar.Button.Stream.Stop"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStopping_RemainsChecked()
    {
        //Arrange — active: Checked=true (synced via active:streamingActive)
        var button = Create(streamingActive: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPING));

        //Assert — isActive(true) == Checked(true) → guard blocks, no shim call
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStopped_UnchecksAndSetsStartText()
    {
        //Arrange — active: Checked=true (synced via active:streamingActive)
        var button = Create(streamingActive: true);

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED));

        //Assert — isActive(false) != Checked(true) → fires
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, false), Times.Once);
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, "Toolbar.Button.Stream.Start"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_SameState_DoesNotCallShim()
    {
        //Arrange — active: Checked=true (synced via active:streamingActive)
        var button = Create(streamingActive: true);

        //Act — same state as current
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert — isActive(true) == Checked(true) → guard blocks
        BtnShimMock.Verify(s => s.SetChecked(StreamBtnPtr, It.IsAny<bool>()), Times.Never);
        BtnShimMock.Verify(s => s.SetText(StreamBtnPtr, It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeState()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert
        BtnShimMock.Verify(s => s.SetChecked(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
        BtnShimMock.Verify(s => s.SetText(It.IsAny<IntPtr>(), It.IsAny<string>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideStartStreaming_HidesButton()
    {
        //Arrange
        var button = Create();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartStreaming = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(StreamBtnPtr, false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowStartStreaming_ShowsButton()
    {
        //Arrange — hide first then clear
        var button = Create();
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartStreaming = true }));
        WidgetMock.Invocations.Clear();

        //Act
        button.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideStartStreaming = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(StreamBtnPtr, true), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnClicked
    // -----------------------------------------------------------------------

    [Test]
    public void OnClicked_WhenIdle_StartsStreaming()
    {
        //Arrange
        var button = CreateTestable();

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.StreamingStart(), Times.Once);
    }

    [Test]
    public void OnClicked_WhenActive_StopsStreaming()
    {
        //Arrange — StreamButton has no confirmation modal; always stops immediately
        var button = CreateTestable(streamingActive: true);

        //Act
        button.SimulateClick();

        //Assert
        FrontendMock.Verify(f => f.StreamingStop(), Times.Once);
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

    private sealed class StreamButtonTestable(
        IQtControlContext ctx,
        ILocalizationService locale,
        IObsFrontendApi frontend,
        IQMessageBoxShim msgBox,
        IPubSubService pubSub,
        IQAbstractButtonShim btnShim,
        IQWidgetShim widgetShim)
        : StreamButton(ctx, locale, frontend, msgBox, pubSub, btnShim, widgetShim)
    {
        public void SimulateClick() => OnClicked(false);
    }
}
