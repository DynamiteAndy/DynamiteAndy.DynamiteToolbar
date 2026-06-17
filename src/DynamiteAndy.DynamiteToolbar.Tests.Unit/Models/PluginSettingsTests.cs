using DynamiteAndy.DynamiteToolbar.Models;
using ShimPluginSettings = DynamiteAndy.DynamiteToolbar.Models.Shim.PluginSettings;

namespace DynamiteAndy.DynamiteToolbar.Tests.Unit.Models;

[TestFixture]
public class PluginSettingsTests
{
    // -----------------------------------------------------------------------
    // Default state
    // -----------------------------------------------------------------------

    [Test]
    public void DefaultConstructor_AllPropertiesFalse()
    {
        //Arrange + Act
        var settings = new PluginSettings();

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.HideFloat, Is.False);
            Assert.That(settings.HideStudioMode, Is.False);
            Assert.That(settings.HideVirtualCamera, Is.False);
            Assert.That(settings.HideReplayBuffer, Is.False);
            Assert.That(settings.HideStartRecording, Is.False);
            Assert.That(settings.HideStartStreaming, Is.False);
            Assert.That(settings.HideStreamingLabel, Is.False);
            Assert.That(settings.HideRecordingLabel, Is.False);
            Assert.That(settings.HideCurrentSceneLabel, Is.False);
            Assert.That(settings.HideCpuLabel, Is.False);
            Assert.That(settings.HideFpsLabel, Is.False);
            Assert.That(settings.HideDroppedFramesLabel, Is.False);
            Assert.That(settings.HideStopRecordingModal, Is.False);
        }
    }

    // -----------------------------------------------------------------------
    // FromShim — value 1 → true, 0 → false
    // -----------------------------------------------------------------------

    [Test]
    public void FromShim_AllOnes_AllPropertiesTrue()
    {
        //Arrange
        var shim = new ShimPluginSettings
        {
            HideFloat = 1,
            HideStudioMode = 1,
            HideVirtualCamera = 1,
            HideReplayBuffer = 1,
            HideStartRecording = 1,
            HideStartStreaming = 1,
            HideStreamingLabel = 1,
            HideRecordingLabel = 1,
            HideCurrentSceneLabel = 1,
            HideCpuLabel = 1,
            HideFpsLabel = 1,
            HideDroppedFramesLabel = 1,
            HideKbpsLabel = 1,
            HideStopRecordingModal = 1,
        };

        //Act
        var result = PluginSettings.FromShim(shim);

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HideFloat, Is.True);
            Assert.That(result.HideStudioMode, Is.True);
            Assert.That(result.HideVirtualCamera, Is.True);
            Assert.That(result.HideReplayBuffer, Is.True);
            Assert.That(result.HideStartRecording, Is.True);
            Assert.That(result.HideStartStreaming, Is.True);
            Assert.That(result.HideStreamingLabel, Is.True);
            Assert.That(result.HideRecordingLabel, Is.True);
            Assert.That(result.HideCurrentSceneLabel, Is.True);
            Assert.That(result.HideCpuLabel, Is.True);
            Assert.That(result.HideFpsLabel, Is.True);
            Assert.That(result.HideDroppedFramesLabel, Is.True);
            Assert.That(result.HideKbpsLabel, Is.True);
            Assert.That(result.HideStopRecordingModal, Is.True);
        }
    }

    [Test]
    public void FromShim_AllZeros_AllPropertiesFalse()
    {
        //Arrange — all fields default to 0
        var shim = new ShimPluginSettings();

        //Act
        var result = PluginSettings.FromShim(shim);

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HideFloat, Is.False);
            Assert.That(result.HideStudioMode, Is.False);
            Assert.That(result.HideVirtualCamera, Is.False);
            Assert.That(result.HideReplayBuffer, Is.False);
            Assert.That(result.HideStartRecording, Is.False);
            Assert.That(result.HideStartStreaming, Is.False);
            Assert.That(result.HideStreamingLabel, Is.False);
            Assert.That(result.HideRecordingLabel, Is.False);
            Assert.That(result.HideCurrentSceneLabel, Is.False);
            Assert.That(result.HideCpuLabel, Is.False);
            Assert.That(result.HideFpsLabel, Is.False);
            Assert.That(result.HideDroppedFramesLabel, Is.False);
            Assert.That(result.HideKbpsLabel, Is.False);
            Assert.That(result.HideStopRecordingModal, Is.False);
        }
    }

    [Test]
    public void FromShim_MixedValues_MapsCorrectly()
    {
        //Arrange
        var shim = new ShimPluginSettings
        {
            HideFloat = 1,
            HideStudioMode = 0,
            HideFpsLabel = 1,
        };

        //Act
        var result = PluginSettings.FromShim(shim);

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HideFloat, Is.True);
            Assert.That(result.HideStudioMode, Is.False);
            Assert.That(result.HideFpsLabel, Is.True);
        }
    }

    // -----------------------------------------------------------------------
    // ToShim — true → 1, false → 0
    // -----------------------------------------------------------------------

    [Test]
    public void ToShim_AllTrue_AllShimFieldsAreOne()
    {
        //Arrange
        var settings = new PluginSettings
        {
            HideFloat = true,
            HideStudioMode = true,
            HideVirtualCamera = true,
            HideReplayBuffer = true,
            HideStartRecording = true,
            HideStartStreaming = true,
            HideStreamingLabel = true,
            HideRecordingLabel = true,
            HideCurrentSceneLabel = true,
            HideCpuLabel = true,
            HideFpsLabel = true,
            HideDroppedFramesLabel = true,
            HideKbpsLabel = true,
            HideStopRecordingModal = true,
        };

        //Act
        var shim = settings.ToShim();

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(shim.HideFloat, Is.EqualTo(1));
            Assert.That(shim.HideStudioMode, Is.EqualTo(1));
            Assert.That(shim.HideVirtualCamera, Is.EqualTo(1));
            Assert.That(shim.HideReplayBuffer, Is.EqualTo(1));
            Assert.That(shim.HideStartRecording, Is.EqualTo(1));
            Assert.That(shim.HideStartStreaming, Is.EqualTo(1));
            Assert.That(shim.HideStreamingLabel, Is.EqualTo(1));
            Assert.That(shim.HideRecordingLabel, Is.EqualTo(1));
            Assert.That(shim.HideCurrentSceneLabel, Is.EqualTo(1));
            Assert.That(shim.HideCpuLabel, Is.EqualTo(1));
            Assert.That(shim.HideFpsLabel, Is.EqualTo(1));
            Assert.That(shim.HideDroppedFramesLabel, Is.EqualTo(1));
            Assert.That(shim.HideKbpsLabel, Is.EqualTo(1));
            Assert.That(shim.HideStopRecordingModal, Is.EqualTo(1));
        }
    }

    [Test]
    public void ToShim_AllFalse_AllShimFieldsAreZero()
    {
        //Arrange — all false by default
        var settings = new PluginSettings();

        //Act
        var shim = settings.ToShim();

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(shim.HideFloat, Is.Zero);
            Assert.That(shim.HideStudioMode, Is.Zero);
            Assert.That(shim.HideVirtualCamera, Is.Zero);
            Assert.That(shim.HideReplayBuffer, Is.Zero);
            Assert.That(shim.HideStartRecording, Is.Zero);
            Assert.That(shim.HideStartStreaming, Is.Zero);
            Assert.That(shim.HideStreamingLabel, Is.Zero);
            Assert.That(shim.HideRecordingLabel, Is.Zero);
            Assert.That(shim.HideCurrentSceneLabel, Is.Zero);
            Assert.That(shim.HideCpuLabel, Is.Zero);
            Assert.That(shim.HideFpsLabel, Is.Zero);
            Assert.That(shim.HideDroppedFramesLabel, Is.Zero);
            Assert.That(shim.HideKbpsLabel, Is.Zero);
            Assert.That(shim.HideStopRecordingModal, Is.Zero);
        }
    }

    // -----------------------------------------------------------------------
    // Round-trip: FromShim → ToShim must be lossless
    // -----------------------------------------------------------------------

    [Test]
    public void RoundTrip_FromShimThenToShim_PreservesAllValues()
    {
        //Arrange
        var original = new ShimPluginSettings
        {
            HideFloat = 1,
            HideStudioMode = 0,
            HideVirtualCamera = 1,
            HideReplayBuffer = 0,
            HideStartRecording = 1,
            HideStartStreaming = 0,
            HideStreamingLabel = 1,
            HideRecordingLabel = 0,
            HideCurrentSceneLabel = 1,
            HideCpuLabel = 0,
            HideFpsLabel = 1,
            HideDroppedFramesLabel = 0,
            HideKbpsLabel = 1,
            HideStopRecordingModal = 1,
        };

        //Act
        var result = PluginSettings.FromShim(original).ToShim();

        //Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HideFloat, Is.EqualTo(original.HideFloat));
            Assert.That(result.HideStudioMode, Is.EqualTo(original.HideStudioMode));
            Assert.That(result.HideVirtualCamera, Is.EqualTo(original.HideVirtualCamera));
            Assert.That(result.HideReplayBuffer, Is.EqualTo(original.HideReplayBuffer));
            Assert.That(result.HideStartRecording, Is.EqualTo(original.HideStartRecording));
            Assert.That(result.HideStartStreaming, Is.EqualTo(original.HideStartStreaming));
            Assert.That(result.HideStreamingLabel, Is.EqualTo(original.HideStreamingLabel));
            Assert.That(result.HideRecordingLabel, Is.EqualTo(original.HideRecordingLabel));
            Assert.That(result.HideCurrentSceneLabel, Is.EqualTo(original.HideCurrentSceneLabel));
            Assert.That(result.HideCpuLabel, Is.EqualTo(original.HideCpuLabel));
            Assert.That(result.HideFpsLabel, Is.EqualTo(original.HideFpsLabel));
            Assert.That(result.HideDroppedFramesLabel, Is.EqualTo(original.HideDroppedFramesLabel));
            Assert.That(result.HideKbpsLabel, Is.EqualTo(original.HideKbpsLabel));
            Assert.That(result.HideStopRecordingModal, Is.EqualTo(original.HideStopRecordingModal));
        }
    }
}
