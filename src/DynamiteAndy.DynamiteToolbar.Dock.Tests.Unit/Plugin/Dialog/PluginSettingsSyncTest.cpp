#include <gtest/gtest.h>

// PluginSettings is a plain struct — no Qt, no OBS needed
#include "Plugin/Dialog/SettingsDialogShim.h"

// Pull in only the sync logic by reproducing the minimal test harness.
// The actual sync() method is private inside SettingsDialog, but its
// behaviour is fully observable through writeToUi/readFromUi on a real
// dialog. Since that requires a QApplication we test the struct invariants
// here and leave the dialog round-trip to the Qt-backed test.

TEST(PluginSettingsStructTest, DefaultInitialisedToZero)
{
	// Arrange + Act
	PluginSettings s{};

	// Assert
	EXPECT_EQ(s.hideFloat, 0);
	EXPECT_EQ(s.hideStudioMode, 0);
	EXPECT_EQ(s.hideVirtualCamera, 0);
	EXPECT_EQ(s.hideReplayBuffer, 0);
	EXPECT_EQ(s.hideStartRecording, 0);
	EXPECT_EQ(s.hideStartStreaming, 0);
	EXPECT_EQ(s.hideStreamingLabel, 0);
	EXPECT_EQ(s.hideRecordingLabel, 0);
	EXPECT_EQ(s.hideCurrentSceneLabel, 0);
	EXPECT_EQ(s.hideCpuLabel, 0);
	EXPECT_EQ(s.hideFpsLabel, 0);
	EXPECT_EQ(s.hideDroppedFramesLabel, 0);
	EXPECT_EQ(s.hideKbpsLabel, 0);
	EXPECT_EQ(s.hideStopRecordingModal, 0);
}

TEST(PluginSettingsStructTest, AllHiddenCanBeSet)
{
	// Arrange
	PluginSettings s{};

	// Act
	s.hideFloat = 1;
	s.hideStudioMode = 1;
	s.hideVirtualCamera = 1;
	s.hideReplayBuffer = 1;
	s.hideStartRecording = 1;
	s.hideStartStreaming = 1;
	s.hideStreamingLabel = 1;
	s.hideRecordingLabel = 1;
	s.hideCurrentSceneLabel = 1;
	s.hideCpuLabel = 1;
	s.hideFpsLabel = 1;
	s.hideDroppedFramesLabel = 1;
	s.hideKbpsLabel = 1;
	s.hideStopRecordingModal = 1;

	// Assert
	EXPECT_EQ(s.hideFloat, 1);
	EXPECT_EQ(s.hideKbpsLabel, 1);
	EXPECT_EQ(s.hideStopRecordingModal, 1);
}

TEST(PluginSettingsStructTest, PartialUpdateDoesNotAffectOtherFields)
{
	// Arrange
	PluginSettings s{};

	// Act
	s.hideFloat = 1;

	// Assert
	EXPECT_EQ(s.hideFloat, 1);
	EXPECT_EQ(s.hideStudioMode, 0);
	EXPECT_EQ(s.hideReplayBuffer, 0);
	EXPECT_EQ(s.hideKbpsLabel, 0);
}
