include_guard(GLOBAL)
include(FetchContent)

file(READ "${CMAKE_SOURCE_DIR}/buildspec.json" buildspec)
string(JSON _gtest_version GET ${buildspec} dependencies gtest version)
string(JSON _gtest_url GET ${buildspec} dependencies gtest baseUrl)

FetchContent_Declare(
  googletest
  URL "${_gtest_url}/v${_gtest_version}.tar.gz"
  DOWNLOAD_EXTRACT_TIMESTAMP TRUE
  DOWNLOAD_DIR "${CMAKE_SOURCE_DIR}/.deps"
  SOURCE_DIR
  "${CMAKE_SOURCE_DIR}/.deps/googletest-${_gtest_version}"
)

# Prevent GTest from overriding compiler/linker settings on Windows
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)
# Don't install GTest alongside the plugin
set(INSTALL_GTEST OFF CACHE BOOL "" FORCE)

FetchContent_MakeAvailable(googletest)
