#include "FrameMonitorDockWidget.h"
#include "../FlowLayout.h"

FrameMonitorDockWidget::FrameMonitorDockWidget(QWidget *parent) : QFrame(parent)
{
	setupUi(this);

	FlowLayout *flow = new FlowLayout(flowContainer, 0, 0, 0);
	flowContainer->setLayout(flow);

	flow->addWidget(wStreamingGroup);
	flow->addWidget(lblStreamingSeparator);

	flow->addWidget(wRecordingGroup);
	flow->addWidget(lblRecordingSeparator);

	flow->addWidget(wCurrentSceneGroup);
	flow->addWidget(lblCurrentSceneSeparator);

	flow->addWidget(wCpuGroup);
	flow->addWidget(lblCpuSeparator);

	flow->addWidget(wFpsGroup);
	flow->addWidget(lblFpsSeparator);

	flow->addWidget(wDroppedFramesGroup);
	flow->addWidget(lblDroppedFramesSeparator);

	flow->addWidget(wKbpsGroup);
}

bool FrameMonitorDockWidget::hasHeightForWidth() const
{
	if (layout())
		return layout()->hasHeightForWidth();

	return false;
}

int FrameMonitorDockWidget::heightForWidth(int width) const
{
	if (layout() && layout()->hasHeightForWidth())
		return layout()->heightForWidth(width);
	return QFrame::heightForWidth(width);
}

QSize FrameMonitorDockWidget::sizeHint() const
{
	if (layout())
		return layout()->sizeHint();
	return QFrame::sizeHint();
}

void FrameMonitorDockWidget::resizeEvent(QResizeEvent *event)
{
	QFrame::resizeEvent(event);
}

static DockFrameMonitorLabel makeLabelGroup(void *layout, void *name, void *delimiter, void *value,
					    void *separator = nullptr)
{
	DockFrameMonitorLabel labelGroup{};

	labelGroup.containerHLayoutPtr = layout;
	labelGroup.nameLblPtr = name;
	labelGroup.delimiterLblPtr = delimiter;
	labelGroup.valueLblPtr = value;
	labelGroup.separatorLblPtr = separator;

	return labelGroup;
}

DockFrameMonitor FrameMonitorDockWidget::populate()
{
	DockFrameMonitor shim{};

	shim.monitorFramePtr = static_cast<void *>(this);

	shim.streaming = makeLabelGroup(hLayoutStreaming, lblStreamingName, lblStreamingDelimiter, lblStreamingValue,
					lblStreamingSeparator);
	shim.recording = makeLabelGroup(hLayoutRecording, lblRecordingName, lblRecordingDelimiter, lblRecordingValue,
					lblRecordingSeparator);
	shim.currentScene = makeLabelGroup(hLayoutCurrentScene, lblCurrentSceneName, lblCurrentSceneDelimiter,
					   lblCurrentSceneValue, lblCurrentSceneSeparator);
	shim.cpu = makeLabelGroup(hLayoutCpu, lblCpuName, lblCpuDelimiter, lblCpuValue, lblCpuSeparator);
	shim.fps = makeLabelGroup(hLayoutFps, lblFpsName, lblFpsDelimiter, lblFpsValue, lblFpsSeparator);
	shim.droppedFrames = makeLabelGroup(hLayoutDroppedFrames, lblDroppedFramesName, lblDroppedFramesDelimiter,
					    lblDroppedFramesValue, lblDroppedFramesSeparator);

	shim.kbps = makeLabelGroup(hLayoutKbps, lblKbpsName, lblKbpsDelimiter, lblKbpsValue);

	return shim;
}
