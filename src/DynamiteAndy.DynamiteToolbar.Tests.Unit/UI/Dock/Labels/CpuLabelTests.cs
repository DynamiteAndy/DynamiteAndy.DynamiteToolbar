using System.Reflection;
using DynamiteAndy.DynamiteToolbar.Models.Events;
using DynamiteAndy.DynamiteToolbar.Models.Shim;
using DynamiteAndy.DynamiteToolbar.UI.Dock.Labels;
using Moq;
using PluginSettings = DynamiteAndy.DynamiteToolbar.Models.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.UI.Dock.Labels;

[TestFixture]
public class CpuLabelTests : LabelTestBase
{
    private static readonly IntPtr DummyCpuHandle = new(0x98765);

    protected override PluginDock BuildPluginDock() => new()
    {
        Monitor = new DockFrameMonitor { Cpu = MakeDockFrameMonitorLabel() }
    };

    protected override string ExpectedNameKey => "Toolbar.Label.CPU";

    protected override void CreateForNameLabelTest() => _ = Create(clearInvocations: false);

    private CpuLabel Create(
        IntPtr? initialHandle = null,
        bool visible = true,
        bool clearInvocations = true)
    {
        var handleToReturn = initialHandle ?? DummyCpuHandle;

        PlatformMock.Setup(p => p.OSCpuUsageInfoStart()).Returns(handleToReturn);

        return CreateLabel(() => new CpuLabel(
            Context, PlatformMock.Object, LocaleMock.Object, PubSubMock.Object,
            LabelMock.Object, WidgetMock.Object), visible, clearInvocations);
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Test]
    public void Constructor_WhenHandleNotInitialized_InitializesCpuUsageTracking()
    {
        //Arrange + Act
        _ = Create(DummyCpuHandle);

        //Assert
        PlatformMock.Verify(p => p.OSCpuUsageInfoStart(), Times.Once);
    }

    [Test]
    public void Constructor_WhenHandleAlreadyInitialized_DoesNotRestartTracking()
    {
        //Arrange
        _ = Create(DummyCpuHandle);
        PlatformMock.Invocations.Clear();

        //Act
        _ = new CpuLabel(
            Context, PlatformMock.Object, LocaleMock.Object, PubSubMock.Object,
            LabelMock.Object, WidgetMock.Object);

        //Assert
        PlatformMock.Verify(p => p.OSCpuUsageInfoStart(), Times.Never);
    }

    [Test]
    public void Constructor_SubscribesToExpectedEvents()
    {
        //Arrange + Act
        _ = Create();

        //Assert
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Subscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnHeartbeatEvent — state tracking updates
    // -----------------------------------------------------------------------

    [Test]
    public void OnHeartbeatEvent_WithValidHandle_QueriesUsageAndFormatsText()
    {
        //Arrange
        var label = Create();
        PlatformMock.Setup(p => p.OSCpuUsageInfoQuery(DummyCpuHandle)).Returns(12.345);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert — expect formatted value with single-digit precision
        LabelMock.Verify(s => s.SetText(ValuePtr, "12.3%"), Times.Once);
    }

    [Test]
    public void OnHeartbeatEvent_WithZeroHandle_SetsFallbackText()
    {
        //Arrange — force initialization handle assignment to be Zero
        var label = Create(IntPtr.Zero);

        //Act
        label.OnHeartbeatEvent(new HeartbeatEvent());

        //Assert
        LabelMock.Verify(s => s.SetText(ValuePtr, "0.0%"), Times.Once);
        PlatformMock.Verify(p => p.OSCpuUsageInfoQuery(It.IsAny<IntPtr>()), Times.Never);
    }

    // -----------------------------------------------------------------------
    // OnSettingsChangedEvent — settings gate visibility
    // -----------------------------------------------------------------------

    [Test]
    public void OnSettingsChangedEvent_HideCpuLabel_HidesLabel()
    {
        //Arrange
        var label = Create();

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideCpuLabel = true }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), false), Times.AtLeastOnce);
    }

    [Test]
    public void OnSettingsChangedEvent_ShowCpuLabel_ShowsLabel()
    {
        //Arrange — start hidden so a single settings event is enough to assert show
        var label = Create(visible: false);

        //Act
        label.OnSettingsChangedEvent(new SettingsChangedEvent(new PluginSettings { HideCpuLabel = false }));

        //Assert
        WidgetMock.Verify(s => s.SetVisible(It.IsAny<IntPtr>(), true), Times.AtLeastOnce);
    }

    // -----------------------------------------------------------------------
    // Dispose — allocation boundary control
    // -----------------------------------------------------------------------

    [Test]
    public void Dispose_WithValidHandle_DestroysCpuInfoAndClearsStaticField()
    {
        //Arrange
        var label = Create(DummyCpuHandle);

        //Act
        label.Dispose();

        //Assert
        PlatformMock.Verify(p => p.OSCpuUsageInfoDestroy(DummyCpuHandle), Times.Once);

        // Verify the static field was reset to Zero by confirming a subsequent construction re-calls Start()
        PlatformMock.Invocations.Clear();
        _ = Create(DummyCpuHandle);
        PlatformMock.Verify(p => p.OSCpuUsageInfoStart(), Times.Once);
    }

    [Test]
    public void Dispose_WithZeroHandle_DoesNotAttemptToDestroy()
    {
        //Arrange
        var label = Create(IntPtr.Zero);

        //Act
        label.Dispose();

        //Assert
        PlatformMock.Verify(p => p.OSCpuUsageInfoDestroy(It.IsAny<IntPtr>()), Times.Never);
    }

    [Test]
    public void Dispose_UnsubscribesFromAllEvents()
    {
        //Arrange
        var label = Create();

        //Act
        label.Dispose();

        //Assert
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<HeartbeatEvent>>()), Times.Once);
        PubSubMock.Verify(p => p.Unsubscribe(It.IsAny<Action<SettingsChangedEvent>>()), Times.Once);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the private static handle after EVERY test execution
        // to prevent leakages and ensure total isolation between runs.
        var field = typeof(CpuLabel).GetField("_cpuInfo", BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, IntPtr.Zero);
    }
}
