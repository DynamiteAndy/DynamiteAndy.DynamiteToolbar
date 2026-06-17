using DynamiteAndy.DynamiteToolbar.Interop.Obs;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using ObsInterop;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class KbpsLabelTests : LabelTestBase
{
    private Mock<IObsPlatformApi> _platformApiMock = null!;

    [SetUp]
    public override void SetUp()
    {
        _platformApiMock = new Mock<IObsPlatformApi>();
        base.SetUp();
    }

    private static ulong ToNanoseconds(TimeSpan ts) => (ulong)ts.Ticks * 100;

    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { Kbps = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.Kbps";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private KbpsLabel Create(bool streamingActive = false, bool visible = true, bool clearInvocations = true)
    {
        FrontendMock.Setup(f => f.StreamingActive).Returns(streamingActive);

        return CreateLabel(() => new KbpsLabel(
            Context, ObsApiMock.Object, FrontendMock.Object, _platformApiMock.Object,
            LocaleMock.Object, PubSubMock.Object,
            LabelMock.Object, WidgetMock.Object), visible, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenNotStreaming_SettingsEventKeepsLabelHidden()
    {
        //Arrange — constructor stores _visibilityFromStreaming=false; verify via settings event
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideKbpsLabel = false }));

        //Assert — streaming=false gates visibility regardless of setting
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.Never);
    }

    [Test]
    public void Constructor_WhenStreaming_SettingsEventShowsLabel()
    {
        //Arrange — constructor stores _visibilityFromStreaming=true; verify via settings event
        var label = Create(streamingActive: true, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideKbpsLabel = false }));

        //Assert — streaming=true allows visibility when setting also allows
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // OnObsFrontendEvent — streaming gates visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnObsFrontendEvent_StreamingStarting_ShowsLabel()
    {
        //Arrange — start hidden so the Visible guard doesn't block the shim call
        var label = Create(visible: false);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTING));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStarted_ShowsLabel()
    {
        //Arrange — start hidden so the Visible guard doesn't block the shim call
        var label = Create(visible: false);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STARTED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnObsFrontendEvent_StreamingStopped_HidesLabelAndResetsValue()
    {
        //Arrange
        var label = Create(streamingActive: true);

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_STREAMING_STOPPED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
        LabelMock.Verify(s => s.SetText(ValuePtr, "0"), Times.Once);
    }

    [Test]
    public void OnObsFrontendEvent_UnrelatedEvent_DoesNotChangeVisibility()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnObsFrontendEvent(new ObsFrontendEvent(obs_frontend_event.OBS_FRONTEND_EVENT_RECORDING_STARTED));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), It.IsAny<bool>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent — settings gate visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideKbpsLabel_HidesLabel()
    {
        //Arrange
        var label = Create(streamingActive: true);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideKbpsLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowKbpsLabel_WhileStreaming_ShowsLabel()
    {
        //Arrange — start hidden while streaming so a single settings event is enough to assert show
        var label = Create(streamingActive: true, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideKbpsLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowKbpsLabel_WhileNotStreaming_RemainsHidden()
    {
        //Arrange — start hidden while not streaming so a single settings event is enough to assert hidden
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideKbpsLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_WhenNotStreaming_DoesNothing()
    {
        //Arrange
        var label = Create(streamingActive: false, visible: false);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        FrontendMock.Verify(f => f.GetStreamingOutput(), Times.Never);
    }

    [Test]
    public void OnHeartbeatEvent_FirstTick_EstablishesBaselineAndSetsZero()
    {
        //Arrange
        var label = Create(streamingActive: true);
        var dummyOutput = new IntPtr(0x12345);
        FrontendMock.Setup(f => f.GetStreamingOutput()).Returns(dummyOutput);
        ObsApiMock.Setup(a => a.OutputGetTotalBytes(dummyOutput)).Returns(5000UL);
        _platformApiMock.Setup(p => p.OSGetTimeNs()).Returns(ToNanoseconds(TimeSpan.FromSeconds(1)));

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "0"), Times.Once);
        ObsApiMock.Verify(a => a.OutputRelease(dummyOutput), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_SubsequentTicks_CalculatesExpectedKbps()
    {
        // Arrange
        var label = Create(streamingActive: true);
        var dummyOutput = new IntPtr(0x12345);
        FrontendMock.Setup(f => f.GetStreamingOutput()).Returns(dummyOutput);

        int bytesCallCount = 0;
        // To get 8 kbps over 2 seconds, we need a 2000-byte difference (16000 bits / 2s / 1000 = 8 kbps)
        ObsApiMock.Setup(a => a.OutputGetTotalBytes(dummyOutput))
            .Returns(() => ++bytesCallCount == 1 ? 1000UL : 3000UL);

        var timeBaseline = TimeSpan.FromSeconds(1);
        var oneSecond = TimeSpan.FromSeconds(1);
        var twoSeconds = TimeSpan.FromSeconds(2);

        _platformApiMock.SetupSequence(p => p.OSGetTimeNs())
                .Returns(ToNanoseconds(timeBaseline))
                .Returns(ToNanoseconds(timeBaseline + oneSecond))
                .Returns(ToNanoseconds(timeBaseline + oneSecond + twoSeconds))
                .Returns(ToNanoseconds(timeBaseline + oneSecond + twoSeconds));

        // Act — tick 1 establishes baseline, tick 2 calculates (16000 bits / 2 seconds / 1000 = 8 kbps)
        label.OnHeartbeatEvent(new HeartbeatEvent());
        label.OnHeartbeatEvent(new HeartbeatEvent());

        // Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "8"), Times.Once);
        ObsApiMock.Verify(a => a.OutputRelease(dummyOutput), Times.Exactly(2));
    }

    // -----------------------------------------------------------------------
    // Dispose
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_UnsubscribesFromAllEvents()
    {
        //Arrange
        var label = Create();

        //Act
        label.Dispose();

        //Assert
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<ObsFrontendEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }
}