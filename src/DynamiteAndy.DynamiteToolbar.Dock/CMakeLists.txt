# Protect against older CMake versions choking on string(JSON)
cmake_minimum_required(VERSION 3.19)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/../../buildspec.json" buildspec)
string(JSON _cmake_min GET ${buildspec} cmakeMinimumVersion)
cmake_minimum_required(VERSION ${_cmake_min})

# ---------------------------------------------------------------------------
# Project identity (standalone build override)
# ---------------------------------------------------------------------------
if(CMAKE_SOURCE_DIR STREQUAL CMAKE_CURRENT_SOURCE_DIR)
  string(JSON _version GET ${buildspec} version)
  project(DynamiteAndy.DynamiteToolbar.Dock VERSION ${_version} LANGUAGES CXX)
endif()

# ---------------------------------------------------------------------------
# Object library — sources compiled once, shared by the DLL and unit tests
# ---------------------------------------------------------------------------
add_library(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  OBJECT
  Logger.cpp
  Logger.h
  ModuleShim.cpp
  ModuleShim.h
  Plugin/Dialog/SettingsDialogShim.cpp
  Plugin/Dialog/SettingsDialogShim.h
  Plugin/Dock/DockHeaderWidget.h
  Plugin/Dock/DockShim.cpp
  Plugin/Dock/DockShim.h
  Plugin/Dock/DockWrapperWidget.cpp
  Plugin/Dock/DockWrapperWidget.h
  Plugin/Dock/FrameMonitorDockWidget.cpp
  Plugin/Dock/FrameMonitorDockWidget.h
  Plugin/Dock/ToolbarWidget.cpp
  Plugin/Dock/ToolbarWidget.h
  Plugin/FlowLayout.cpp
  Plugin/FlowLayout.h
  Plugin/Menu/ToolsMenuShim.cpp
  Plugin/Menu/ToolsMenuShim.h
  Plugin/Types.h
  QT/QAbstractButtonShim.cpp
  QT/QAbstractButtonShim.h
  QT/QActionShim.cpp
  QT/QActionShim.h
  QT/QDockWidgetShim.cpp
  QT/QDockWidgetShim.h
  QT/QGroupBoxShim.cpp
  QT/QGroupBoxShim.h
  QT/QIconUtils.h
  QT/QLabelShim.cpp
  QT/QLabelShim.h
  QT/QMessageBoxShim.cpp
  QT/QMessageBoxShim.h
  QT/QObjectUtils.h
  QT/QThreadUtils.h
  QT/QWidgetShim.cpp
  QT/QWidgetShim.h
  UI/plugin-dock-header.ui
  UI/plugin-dock-wrapper.ui
  UI/plugin-monitor-panel.ui
  UI/plugin-settings-dialog.ui
  UI/plugin-toolbar.ui
  UI/plugin-resources.qrc
)

set_target_properties(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  PROPERTIES
    CXX_STANDARD 20
    CXX_STANDARD_REQUIRED ON
    CXX_EXTENSIONS OFF
    AUTOMOC ON
    AUTOUIC ON
    AUTORCC ON
    AUTOUIC_SEARCH_PATHS "${CMAKE_CURRENT_SOURCE_DIR}/UI"
)

# Robustly export the Autogen directories using generator expressions.
# This eliminates hardcoded path strings and automatically handles multi-config setups.
target_include_directories(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}"
  INTERFACE
    "$<BUILD_INTERFACE:$<TARGET_PROPERTY:DynamiteAndy.DynamiteToolbar.Dock.Objects,BINARY_DIR>/DynamiteAndy.DynamiteToolbar.Dock.Objects_autogen/include>"
    "$<BUILD_INTERFACE:$<TARGET_PROPERTY:DynamiteAndy.DynamiteToolbar.Dock.Objects,BINARY_DIR>/DynamiteAndy.DynamiteToolbar.Dock.Objects_autogen/include_$<CONFIG>>"
)

target_compile_definitions(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  PUBLIC
    DYNAMITEANDYDYNAMITETOOLBARDOCK_EXPORTS
    $<$<PLATFORM_ID:Windows>:WIN32_LEAN_AND_MEAN
    UNICODE
    _UNICODE>
    PROJECT_VERSION="${PROJECT_VERSION}"
)

# Dependencies
find_package(libobs REQUIRED)
find_package(obs-frontend-api REQUIRED)
find_package(Qt6 COMPONENTS Core Gui Widgets Svg REQUIRED)

target_link_libraries(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  PUBLIC OBS::libobs OBS::obs-frontend-api Qt6::Core Qt6::Gui Qt6::Widgets Qt6::Svg
)

# Silence Clang/AppleClang warnings that Qt headers trigger on macOS
target_compile_options(
  DynamiteAndy.DynamiteToolbar.Dock.Objects
  PRIVATE $<$<CXX_COMPILER_ID:Clang,AppleClang>: -Wno-quoted-include-in-framework-header -Wno-comma >
)

# ---------------------------------------------------------------------------
# DLL — thin wrapper around the object lib
# ---------------------------------------------------------------------------
add_library(DynamiteAndy.DynamiteToolbar.Dock SHARED)

set_target_properties(
  DynamiteAndy.DynamiteToolbar.Dock
  PROPERTIES CXX_STANDARD 20 CXX_STANDARD_REQUIRED ON CXX_EXTENSIONS OFF
)

# Apply plugin customizations and dependencies immediately after target creation
set_target_properties_plugin(DynamiteAndy.DynamiteToolbar.Dock
  PROPERTIES OUTPUT_NAME "dynamiteandy.dynamite-toolbar.dock"
)

add_dependencies(DynamiteAndy.DynamiteToolbar.Dock DynamiteAndy.DynamiteToolbar)
target_link_libraries(DynamiteAndy.DynamiteToolbar.Dock PRIVATE DynamiteAndy.DynamiteToolbar.Dock.Objects)

# Windows-specific link adjustments
if(WIN32)
  target_link_options(DynamiteAndy.DynamiteToolbar.Dock PRIVATE /SUBSYSTEM:WINDOWS /MANIFESTUAC:NO)
endif()

# Linux RPATH configuration
if(UNIX AND NOT APPLE)
  set_target_properties(
    DynamiteAndy.DynamiteToolbar.Dock
    PROPERTIES INSTALL_RPATH "$ORIGIN;$ORIGIN/../.." BUILD_WITH_INSTALL_RPATH TRUE
  )
endif()

# ---------------------------------------------------------------------------
# Install (Safely placed at the bottom after all properties are assigned)
# ---------------------------------------------------------------------------
if(WIN32)
  install(TARGETS DynamiteAndy.DynamiteToolbar.Dock RUNTIME DESTINATION "obs-plugins/64bit")
elseif(APPLE)
  install(TARGETS DynamiteAndy.DynamiteToolbar.Dock LIBRARY DESTINATION "${CMAKE_PROJECT_NAME}.plugin/Contents/MacOS")
else()
  install(TARGETS DynamiteAndy.DynamiteToolbar.Dock LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}/obs-plugins")
endif()
