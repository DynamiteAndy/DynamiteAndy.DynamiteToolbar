cmake_minimum_required(VERSION 3.19)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/buildspec.json" buildspec)
string(JSON _cmake_min GET ${buildspec} cmakeMinimumVersion)
cmake_minimum_required(VERSION ${_cmake_min})

include("${CMAKE_CURRENT_SOURCE_DIR}/cmake/common/bootstrap.cmake" NO_POLICY_SCOPE)

project(dynamiteandy.dynamite-toolbar VERSION ${PLUGIN_VERSION})

option(ENABLE_FRONTEND_API "Use obs-frontend-api for UI functionality" ON)
option(ENABLE_QT "Use Qt functionality" ON)
option(ENABLE_TESTS "Build unit tests" OFF)

if(ENABLE_TESTS)
  enable_testing()
endif()

include(compilerconfig)
include(defaults)
include(helpers)

# ------------------------------------------------------------
# .NET NativeAOT configuration (passed down to sub-CMake)
# ------------------------------------------------------------

if(NOT DEFINED DOTNET_RID)
  if(WIN32)
    set(DOTNET_RID "win-x64")
  elseif(APPLE)
    if(CMAKE_OSX_ARCHITECTURES STREQUAL "arm64;x86_64" OR CMAKE_OSX_ARCHITECTURES STREQUAL "x86_64;arm64")
      # Universal binary: publish each slice separately then lipo in the NativeAOT CMakeLists
      set(DOTNET_RID "osx-arm64")
      set(DOTNET_RID_SECONDARY "osx-x64")
    elseif(CMAKE_OSX_ARCHITECTURES STREQUAL "arm64" OR CMAKE_SYSTEM_PROCESSOR MATCHES "arm64|aarch64")
      set(DOTNET_RID "osx-arm64")
    else()
      set(DOTNET_RID "osx-x64")
    endif()
  else()
    set(DOTNET_RID "linux-x64")
  endif()
endif()

if(WIN32)
  set(DOTNET_PLUGIN_FILENAME "dynamiteandy.dynamite-toolbar.dll")
  set(DOTNET_INSTALL_DEST "obs-plugins/64bit")
elseif(APPLE)
  set(DOTNET_PLUGIN_FILENAME "dynamiteandy.dynamite-toolbar.dylib")
  set(DOTNET_INSTALL_DEST "${CMAKE_PROJECT_NAME}.plugin/Contents/MacOS")
else()
  set(DOTNET_PLUGIN_FILENAME "dynamiteandy.dynamite-toolbar.so")
  set(DOTNET_INSTALL_DEST "${CMAKE_INSTALL_LIBDIR}/obs-plugins")
endif()

set(DOTNET_OUTPUT_DIR "${CMAKE_BINARY_DIR}/dotnet-publish")
set(DOTNET_LOG_FILE "${CMAKE_BINARY_DIR}/dotnet-publish.log")

# ------------------------------------------------------------
# Sub-projects
# ------------------------------------------------------------

# Core Targets
add_subdirectory(src/DynamiteAndy.DynamiteToolbar)
add_subdirectory(src/DynamiteAndy.DynamiteToolbar.Dock)

# Test Targets (Only included if testing is enabled)
if(ENABLE_TESTS)
  add_subdirectory(src/DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit)
  add_subdirectory(src/DynamiteAndy.DynamiteToolbar.Tests.Unit)
endif()

# ------------------------------------------------------------
# Install the NativeAOT binary alongside the plugin
# ------------------------------------------------------------

install(FILES "${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}" DESTINATION "${DOTNET_INSTALL_DEST}")

# Run the macOS packager AFTER all dylibs are installed.
# install(SCRIPT ...) was previously in helpers.cmake/set_target_properties_plugin
# but that caused it to run before the dylib install rules were processed.
if(APPLE)
  install(SCRIPT "${CMAKE_BINARY_DIR}/src/DynamiteAndy.DynamiteToolbar.Dock/create-package.cmake")
endif()
