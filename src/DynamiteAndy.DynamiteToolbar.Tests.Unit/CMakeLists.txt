# Protect against older CMake versions choking on string(JSON)
cmake_minimum_required(VERSION 3.19)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/../../buildspec.json" buildspec)
string(JSON _cmake_min GET ${buildspec} cmakeMinimumVersion)
cmake_minimum_required(VERSION ${_cmake_min})

# Defensive check (redundant if skipped by the parent directory, but safe)
if(NOT ENABLE_TESTS)
  return()
endif()

find_program(DOTNET_EXECUTABLE NAMES dotnet)
if(NOT DOTNET_EXECUTABLE)
  message(WARNING "dotnet not found — C# unit tests will not be registered with CTest.")
  return()
endif()

set(DOTNET_TEST_CSPROJ "${CMAKE_CURRENT_SOURCE_DIR}/DynamiteAndy.DynamiteToolbar.Tests.Unit.csproj")

# Cleaned up: Moved the stamp file to the local subdirectory binary folder
set(DOTNET_TEST_STAMP "${CMAKE_CURRENT_BINARY_DIR}/dotnet-test-build.stamp")

file(GLOB_RECURSE DOTNET_TEST_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/*.cs")

# Build the C# test project as part of cmake --build
add_custom_command(
  OUTPUT "${DOTNET_TEST_STAMP}"
  COMMAND "${DOTNET_EXECUTABLE}" build "${DOTNET_TEST_CSPROJ}" --configuration Release
  COMMAND ${CMAKE_COMMAND} -E touch "${DOTNET_TEST_STAMP}"
  DEPENDS "${DOTNET_TEST_CSPROJ}" ${DOTNET_TEST_SOURCES}
  COMMENT "Building C# unit tests"
  VERBATIM
)

add_custom_target(build_dotnet_tests ALL DEPENDS "${DOTNET_TEST_STAMP}")

# Prevent concurrent MSBuild/dotnet CLI instance collisions during parallel builds
if(TARGET publish_dotnet)
  add_dependencies(build_dotnet_tests publish_dotnet)
endif()

# ------------------------------------------------------------
# CTest Registration
# ------------------------------------------------------------
set(DOTNET_TRX_OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/TestResults/dotnet-test-results.trx")
set(DOTNET_COVERAGE_OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/TestResults")

add_test(
  NAME DynamiteAndy.DynamiteToolbar.Tests.Unit
  COMMAND
    dotnet-coverage collect --settings "${CMAKE_CURRENT_SOURCE_DIR}/.runsettings" --output-format cobertura --output
    "${DOTNET_COVERAGE_OUTPUT}/coverage.cobertura.xml" "${DOTNET_EXECUTABLE}" test "${DOTNET_TEST_CSPROJ}" --no-build
    --configuration Release --results-directory "${DOTNET_COVERAGE_OUTPUT}" --logger
    "trx;LogFileName=${DOTNET_TRX_OUTPUT}" --logger "console;verbosity=normal"
  WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
)

set_tests_properties(DynamiteAndy.DynamiteToolbar.Tests.Unit PROPERTIES LABELS "dotnet")
