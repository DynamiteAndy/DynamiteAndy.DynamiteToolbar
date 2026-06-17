#pragma once

struct DockFrameMonitorLabel {
	void *containerHLayoutPtr;
	void *nameLblPtr;
	void *delimiterLblPtr;
	void *valueLblPtr;
	void *separatorLblPtr;
};

struct DockFrameMonitor {
	void *monitorFramePtr = nullptr;
	DockFrameMonitorLabel streaming;
	DockFrameMonitorLabel recording;
	DockFrameMonitorLabel currentScene;
	DockFrameMonitorLabel cpu;
	DockFrameMonitorLabel fps;
	DockFrameMonitorLabel droppedFrames;
	DockFrameMonitorLabel kbps;
};

struct DockButtons {
	void *floatBtnPtr;
	void *studioModeBtnPtr;
	void *virtualCameraBtnPtr;
	void *replayBufferBtnPtr;
	void *recordBtnPtr;
	void *recordPausedBtnPtr;
	void *streamBtnPtr;
};

struct DockSpacers {
	void *floatSpacerPtr;
	void *studioModeSpacerPtr;
	void *monitorSpacerPtr;
	void *virtualCameraSpacerPtr;
	void *replayBufferSpacerPtr;
	void *recordSpacerPtr;
};

struct PluginDock {
	void *pluginDockPtr;
	DockFrameMonitor monitor;
	DockButtons buttons;
	DockSpacers spacers;
};
