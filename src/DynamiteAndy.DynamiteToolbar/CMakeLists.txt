cmake_minimum_required(VERSION 3.19)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/../../buildspec.json" buildspec)
string(JSON _cmake_min GET ${buildspec} cmakeMinimumVersion)
cmake_minimum_required(VERSION ${_cmake_min})

# ------------------------------------------------------------
# Required configuration
# ------------------------------------------------------------

if(NOT DEFINED DOTNET_RID)
  message(FATAL_ERROR "DOTNET_RID is not defined (e.g. win-x64, linux-x64).")
endif()

# Simplifies check logic using modern CMake 3.18+ REQUIRED flag
find_program(DOTNET_EXECUTABLE NAMES dotnet REQUIRED)

# Output directory for dotnet publish
set(DOTNET_OUTPUT_DIR "${CMAKE_BINARY_DIR}/dotnet-publish" CACHE PATH "dotnet publish output directory")

# Plugin filename (e.g. MyPlugin.dll / MyPlugin.so)
if(NOT DEFINED DOTNET_PLUGIN_FILENAME)
  message(FATAL_ERROR "DOTNET_PLUGIN_FILENAME is not defined.")
endif()

# Log file
set(DOTNET_LOG_FILE "${CMAKE_BINARY_DIR}/dotnet-publish.log" CACHE FILEPATH "dotnet publish log file")

# ------------------------------------------------------------
# Project file
# ------------------------------------------------------------

set(
  DOTNET_CSPROJ
  "${CMAKE_CURRENT_SOURCE_DIR}/DynamiteAndy.DynamiteToolbar.csproj"
  CACHE FILEPATH
  "Path to the .csproj for the plugin"
)

if(NOT EXISTS "${DOTNET_CSPROJ}")
  message(FATAL_ERROR "Expected project file not found: ${DOTNET_CSPROJ}")
endif()

# ------------------------------------------------------------
# Deterministic dependency tracking
# ------------------------------------------------------------

set(PUBLISH_STAMP "${DOTNET_OUTPUT_DIR}/publish.stamp")

# Watch all C# files for changes
file(GLOB_RECURSE DOTNET_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cs")

# Derive assembly name to safeguard MSBuild from Xcode environment contamination
get_filename_component(DOTNET_ASSEMBLY_NAME "${DOTNET_PLUGIN_FILENAME}" NAME_WLE)

# ------------------------------------------------------------
# Shared dotnet publish command arguments
# ------------------------------------------------------------

set(
  DOTNET_PUBLISH_ARGS
  publish
  "${DOTNET_CSPROJ}"
  -c
  Release
  -r
  "${DOTNET_RID}"
  -o
  "${DOTNET_OUTPUT_DIR}"
  /p:NativeLib=Shared
  /p:SelfContained=true
  /p:PublishAot=true
  /p:AssemblyName=${DOTNET_ASSEMBLY_NAME}
  /p:NativeOutputName=${DOTNET_ASSEMBLY_NAME}
  -v
  normal
)

# FIX: Convert the list elements into a single space-separated string.
# This prevents Linux Bash from treating CMake semicolons (;) as native
# command terminators inside the inline shell string wrapper below.
string(JOIN " " DOTNET_PUBLISH_ARGS_SPACE ${DOTNET_PUBLISH_ARGS})

# ------------------------------------------------------------
# Custom Command Generation
# ------------------------------------------------------------

if(APPLE AND DEFINED DOTNET_RID_SECONDARY)
  # Universal macOS build: publish arm64 and x64 slices separately, then lipo.
  set(_out_arm64 "${CMAKE_BINARY_DIR}/dotnet-publish-arm64")
  set(_out_x64 "${CMAKE_BINARY_DIR}/dotnet-publish-x64")

  set(
    _args_arm64
    publish
    "${DOTNET_CSPROJ}"
    -c
    Release
    -r
    "${DOTNET_RID}"
    -o
    "${_out_arm64}"
    /p:NativeLib=Shared
    /p:SelfContained=true
    /p:PublishAot=true
    /p:AssemblyName=${DOTNET_ASSEMBLY_NAME}
    /p:NativeOutputName=${DOTNET_ASSEMBLY_NAME}
    -v
    normal
  )
  set(
    _args_x64
    publish
    "${DOTNET_CSPROJ}"
    -c
    Release
    -r
    "${DOTNET_RID_SECONDARY}"
    -o
    "${_out_x64}"
    /p:NativeLib=Shared
    /p:SelfContained=true
    /p:PublishAot=true
    /p:AssemblyName=${DOTNET_ASSEMBLY_NAME}
    /p:NativeOutputName=${DOTNET_ASSEMBLY_NAME}
    -v
    normal
  )

  string(JOIN " " _args_arm64_str ${_args_arm64})
  string(JOIN " " _args_x64_str ${_args_x64})

  add_custom_command(
    OUTPUT "${PUBLISH_STAMP}"
    BYPRODUCTS "${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}" "${DOTNET_LOG_FILE}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${DOTNET_OUTPUT_DIR}" "${_out_arm64}" "${_out_x64}"
    COMMAND
      bash -c
      "unset PRODUCT_NAME TARGET_NAME TARGETNAME; \"${DOTNET_EXECUTABLE}\" ${_args_arm64_str} >> \"${DOTNET_LOG_FILE}\" 2>&1 || { echo '--- dotnet publish (${DOTNET_RID}) failed ---' >&2; tail -n 200 \"${DOTNET_LOG_FILE}\" >&2; exit 1; }"
    COMMAND
      bash -c
      "unset PRODUCT_NAME TARGET_NAME TARGETNAME; \"${DOTNET_EXECUTABLE}\" ${_args_x64_str} >> \"${DOTNET_LOG_FILE}\" 2>&1 || { echo '--- dotnet publish (${DOTNET_RID_SECONDARY}) failed ---' >&2; tail -n 200 \"${DOTNET_LOG_FILE}\" >&2; exit 1; }"
    COMMAND
      bash -c
      "lipo -create \"${_out_arm64}/${DOTNET_PLUGIN_FILENAME}\" \"${_out_x64}/${DOTNET_PLUGIN_FILENAME}\" -output \"${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}\" || { echo '--- lipo failed ---' >&2; exit 1; }"
    COMMAND ${CMAKE_COMMAND} -E touch "${PUBLISH_STAMP}"
    DEPENDS "${DOTNET_CSPROJ}" ${DOTNET_SOURCES}
    COMMENT "Publishing NativeAOT C# plugin (universal: ${DOTNET_RID} + ${DOTNET_RID_SECONDARY})"
    VERBATIM
  )
elseif(WIN32)
  add_custom_command(
    OUTPUT "${PUBLISH_STAMP}"
    BYPRODUCTS "${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}" "${DOTNET_LOG_FILE}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${DOTNET_OUTPUT_DIR}"
    COMMAND "${DOTNET_EXECUTABLE}" ${DOTNET_PUBLISH_ARGS} > "${DOTNET_LOG_FILE}" 2>&1
    COMMAND ${CMAKE_COMMAND} -E touch "${PUBLISH_STAMP}"
    DEPENDS "${DOTNET_CSPROJ}" ${DOTNET_SOURCES}
    COMMENT "Publishing NativeAOT C# plugin (${DOTNET_RID})"
    VERBATIM
  )
else()
  add_custom_command(
    OUTPUT "${PUBLISH_STAMP}"
    BYPRODUCTS "${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}" "${DOTNET_LOG_FILE}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${DOTNET_OUTPUT_DIR}"
    COMMAND
      bash -c
      "unset PRODUCT_NAME TARGET_NAME TARGETNAME; \"${DOTNET_EXECUTABLE}\" ${DOTNET_PUBLISH_ARGS_SPACE} > \"${DOTNET_LOG_FILE}\" 2>&1 || { echo '--- dotnet publish failed ---' >&2; tail -n 200 \"${DOTNET_LOG_FILE}\" >&2; exit 1; }"
    COMMAND ${CMAKE_COMMAND} -E touch "${PUBLISH_STAMP}"
    DEPENDS "${DOTNET_CSPROJ}" ${DOTNET_SOURCES}
    COMMENT "Publishing NativeAOT C# plugin (${DOTNET_RID})"
    VERBATIM
  )
endif()

# ------------------------------------------------------------
# Publish target (no ALL — only runs when needed)
# ------------------------------------------------------------

add_custom_target(publish_dotnet DEPENDS "${PUBLISH_STAMP}")

# ------------------------------------------------------------
# Imported library representing the NativeAOT output
# ------------------------------------------------------------

add_library(DynamiteAndy.DynamiteToolbar UNKNOWN IMPORTED GLOBAL)

set_target_properties(
  DynamiteAndy.DynamiteToolbar
  PROPERTIES IMPORTED_LOCATION "${DOTNET_OUTPUT_DIR}/${DOTNET_PLUGIN_FILENAME}"
)

# Ensure the imported binary is built before use
add_dependencies(DynamiteAndy.DynamiteToolbar publish_dotnet)
