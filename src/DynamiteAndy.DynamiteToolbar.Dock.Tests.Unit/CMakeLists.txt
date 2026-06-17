# Protect against older CMake versions choking on string(JSON)
cmake_minimum_required(VERSION 3.19)

file(READ "${CMAKE_CURRENT_SOURCE_DIR}/../../buildspec.json" buildspec)
string(JSON _cmake_min GET ${buildspec} cmakeMinimumVersion)
cmake_minimum_required(VERSION ${_cmake_min})

if(NOT ENABLE_TESTS)
  return()
endif()

# ---------------------------------------------------------------------------
# Project identity (Standalone layout support)
# ---------------------------------------------------------------------------
if(CMAKE_SOURCE_DIR STREQUAL CMAKE_CURRENT_SOURCE_DIR)
  string(JSON _version GET ${buildspec} version)

  project(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit VERSION ${_version} LANGUAGES CXX)

  list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/../../cmake/common")

  # Removed 'Test' component here to support stripped-down OBS binary environments
  find_package(Qt6 REQUIRED COMPONENTS Core Gui Widgets Svg)

  add_subdirectory("${CMAKE_CURRENT_SOURCE_DIR}/../DynamiteAndy.DynamiteToolbar.Dock" DynamiteAndy.DynamiteToolbar.Dock)
endif()

include(gtest)

# ---------------------------------------------------------------------------
# Test Executable Target
# ---------------------------------------------------------------------------
add_executable(
  DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit
  # Mocks
  Mocks/MockObsFrontend.cpp
  Plugin/Dialog/PluginSettingsSyncTest.cpp
  # Qt-backed
  Plugin/FlowLayoutTest.cpp
  Plugin/Dock/FrameMonitorDockWidgetTest.cpp
  Plugin/Dock/DockWrapperWidgetTest.cpp
  Plugin/Dock/DockHeaderWidgetTest.cpp
  Plugin/Dock/ToolbarWidgetTest.cpp
  # Shim tests
  QT/QAbstractButtonShimTest.cpp
  QT/QActionShimTest.cpp
  QT/QWidgetShimTest.cpp
  QT/QLabelShimTest.cpp
  QT/QGroupBoxShimTest.cpp
  QT/QDockWidgetShimTest.cpp
  QT/QIconUtilsTest.cpp
  # OBS-mocked shim tests
  Plugin/Dock/DockShimTest.cpp
  Plugin/Menu/ToolsMenuShimTest.cpp
  QT/QMessageBoxShimTest.cpp
)

set_target_properties(
  DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit
  PROPERTIES CXX_STANDARD 20 CXX_STANDARD_REQUIRED ON AUTOMOC ON
)

target_include_directories(
  DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit
  PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}" "${CMAKE_CURRENT_SOURCE_DIR}/Mocks"
)

# Linked items are explicitly kept to standard layout libraries + GoogleTest
target_link_libraries(
  DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit
  PRIVATE GTest::gtest_main GTest::gmock Qt6::Core Qt6::Gui Qt6::Widgets Qt6::Svg DynamiteAndy.DynamiteToolbar.Dock.Objects
)

# ---------------------------------------------------------------------------
# Code Coverage Instrumentation
# ---------------------------------------------------------------------------
# Two mechanisms depending on toolchain, both ending up at the same place:
# TestResults/coverage.cobertura.xml, matching the C#
# DynamiteAndy.DynamiteToolbar.Tests.Unit output.
#   - GCC/Clang: -fprofile-arcs/-ftest-coverage (compile) + the gcov runtime
#     (link) produce .gcno/.gcda, turned into Cobertura by gcovr below.
#   - MSVC (the windows-x64 preset's cl.exe): /Z7 embeds full debug info in
#     every .obj so OpenCppCoverage can map instructions back to source
#     lines even in a Release build, and /DEBUG keeps the linker from
#     dropping it.
set(DOCK_COVERAGE_ENABLED FALSE)
set(DOCK_COVERAGE_KIND "")

if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang|AppleClang")
  set(DOCK_COVERAGE_ENABLED TRUE)
  set(DOCK_COVERAGE_KIND "gcov")

  # Instrument the code under test (the OBJECT library shared with the
  # production plugin) as well as the test binary's own sources.
  target_compile_options(DynamiteAndy.DynamiteToolbar.Dock.Objects PRIVATE --coverage)
  target_compile_options(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit PRIVATE --coverage)
  target_link_options(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit PRIVATE --coverage)

  # DynamiteAndy.DynamiteToolbar.Dock.Objects' instrumented object files are also
  # linked into the production plugin SHARED library when building in
  # integrated mode with ENABLE_TESTS=ON. Without --coverage at link time
  # there, the gcov runtime symbols those objects reference would be
  # undefined. This only affects ENABLE_TESTS=ON builds; CI release builds
  # have ENABLE_TESTS=OFF and are unaffected.
  if(TARGET DynamiteAndy.DynamiteToolbar.Dock)
    target_link_options(DynamiteAndy.DynamiteToolbar.Dock PRIVATE --coverage)
  endif()
elseif(MSVC)
  set(DOCK_COVERAGE_ENABLED TRUE)
  set(DOCK_COVERAGE_KIND "opencppcoverage")

  # /Z7 keeps each .obj's debug info self-contained (avoids shared-.pdb
  # contention between parallel compiles of DynamiteAndy.DynamiteToolbar.Dock.Objects).
  # /DEBUG ensures the linker still emits a .pdb for the test executable in a
  # Release configuration, which OpenCppCoverage requires to map coverage
  # back to source lines.
  target_compile_options(DynamiteAndy.DynamiteToolbar.Dock.Objects PRIVATE /Z7)
  target_compile_options(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit PRIVATE /Z7)
  target_link_options(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit PRIVATE /DEBUG)
else()
  message(
    STATUS
    "Code coverage is not supported for ${CMAKE_CXX_COMPILER_ID}; DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit will run without coverage instrumentation"
  )
endif()

# ---------------------------------------------------------------------------
# Runtime Dependency Handling (Cross-Platform)
# ---------------------------------------------------------------------------
if(WIN32)
  # Windows requires core DLLs AND the platform plugins next to the executable.
  # Even the "minimal" headless platform requires 'platforms/qminimal.dll' to boot!
  add_custom_command(
    TARGET DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit
    POST_BUILD
    COMMAND
      ${CMAKE_COMMAND} -E copy_if_different "$<TARGET_FILE:Qt6::Core>" "$<TARGET_FILE:Qt6::Gui>"
      "$<TARGET_FILE:Qt6::Widgets>" "$<TARGET_FILE:Qt6::Svg>"
      "$<TARGET_FILE_DIR:DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit>"
    COMMAND
      ${CMAKE_COMMAND} -E copy_directory "$<TARGET_FILE_DIR:Qt6::Core>/../plugins/platforms"
      "$<TARGET_FILE_DIR:DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit>/platforms"
    VERBATIM
  )
endif()

# ---------------------------------------------------------------------------
# Test Discovery & Execution Environment
# ---------------------------------------------------------------------------
include(GoogleTest)

set(DOCK_TEST_PROPERTIES ENVIRONMENT "QT_QPA_PLATFORM=minimal")

# ---------------------------------------------------------------------------
# Code Coverage Reporting -> TestResults/coverage.cobertura.xml
# ---------------------------------------------------------------------------
if(DOCK_COVERAGE_ENABLED)
  set(DOCK_COVERAGE_OUTPUT_DIR "${CMAKE_CURRENT_BINARY_DIR}/TestResults")
  file(MAKE_DIRECTORY "${DOCK_COVERAGE_OUTPUT_DIR}")

  # DynamiteAndy.DynamiteToolbar.Dock.Objects' sources live one directory up from
  # this CMakeLists.txt, both in standalone and integrated layouts.
  get_filename_component(DOCK_SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../DynamiteAndy.DynamiteToolbar.Dock" ABSOLUTE)

  if(DOCK_COVERAGE_KIND STREQUAL "gcov")
    # --- gcovr -> Cobertura (GCC/Clang) -------------------------------------
    # Wires a gcovr-based report into ctest via fixtures:
    #   - "Reset" runs first and clears any .gcda left over from a previous run.
    #   - Every discovered GTest case requires that fixture (so Reset always
    #     runs before them).
    #   - "Report" runs once, after every discovered case has finished, and
    #     converts the resulting .gcno/.gcda into coverage.cobertura.xml.
    find_program(GCOVR_EXECUTABLE NAMES gcovr)

    if(GCOVR_EXECUTABLE)
      set(DOCK_COVERAGE_FIXTURE dock_coverage)

      # gcov itself is compiler-specific: GCC ships its own, Clang needs
      # "llvm-cov gcov" (via xcrun on macOS).
      if(CMAKE_CXX_COMPILER_ID MATCHES "Clang|AppleClang")
        if(APPLE)
          set(DOCK_GCOV_EXECUTABLE "xcrun llvm-cov gcov")
        else()
          set(DOCK_GCOV_EXECUTABLE "llvm-cov gcov")
        endif()
      else()
        set(DOCK_GCOV_EXECUTABLE "gcov")
      endif()

      # find(1) isn't a portable choice (no GNU find on Windows by default),
      # so generate a tiny CMake script to delete stale .gcda files instead.
      set(DOCK_COVERAGE_RESET_SCRIPT "${CMAKE_CURRENT_BINARY_DIR}/gcovr-reset.cmake")
      file(
        WRITE "${DOCK_COVERAGE_RESET_SCRIPT}"
        "file(GLOB_RECURSE _dock_gcda \"${CMAKE_BINARY_DIR}/*.gcda\")\n"
        "if(_dock_gcda)\n"
        "  file(REMOVE \${_dock_gcda})\n"
        "endif()\n"
      )

      add_test(
        NAME DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Reset
        COMMAND "${CMAKE_COMMAND}" -P "${DOCK_COVERAGE_RESET_SCRIPT}"
      )
      set_tests_properties(
        DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Reset
        PROPERTIES FIXTURES_SETUP "${DOCK_COVERAGE_FIXTURE}"
      )

      list(APPEND DOCK_TEST_PROPERTIES FIXTURES_REQUIRED "${DOCK_COVERAGE_FIXTURE}")

      # --filter restricts the report to this plugin's own sources. Qt's
      # generated moc_*/qrc_*/ui_*.h files live under the build tree, outside
      # DOCK_SOURCE_DIR, so they're excluded automatically without needing
      # separate --exclude rules.
      add_test(
        NAME DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Report
        COMMAND
          "${GCOVR_EXECUTABLE}" --root "${DOCK_SOURCE_DIR}" --filter "${DOCK_SOURCE_DIR}/.*" --gcov-executable
          "${DOCK_GCOV_EXECUTABLE}" --print-summary --cobertura-pretty --cobertura
          "${DOCK_COVERAGE_OUTPUT_DIR}/coverage.cobertura.xml" "${CMAKE_BINARY_DIR}"
      )
      set_tests_properties(
        DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Report
        PROPERTIES FIXTURES_CLEANUP "${DOCK_COVERAGE_FIXTURE}"
      )
    else()
      message(
        WARNING
        "gcovr not found — DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit will run without a coverage report. Install gcovr to enable Cobertura coverage output."
      )
    endif()
  elseif(DOCK_COVERAGE_KIND STREQUAL "opencppcoverage")
    # --- OpenCppCoverage -> Cobertura (MSVC) --------------------------------
    # Unlike gcov, OpenCppCoverage wraps the test binary directly: one run
    # both executes every GTest case (no --gtest_filter, so this is
    # independent of the individually-discovered tests below) and writes the
    # report in the same step — no Reset/fixture dance needed.
    find_program(OPENCPPCOVERAGE_EXECUTABLE NAMES OpenCppCoverage)

    if(OPENCPPCOVERAGE_EXECUTABLE)
      # OpenCppCoverage matches --sources against the paths recorded in the
      # .pdb, which on Windows are backslash-separated; a forward-slash
      # filter silently matches nothing and produces an empty report.
      file(TO_NATIVE_PATH "${DOCK_SOURCE_DIR}" DOCK_SOURCE_DIR_NATIVE)

      add_test(
        NAME DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Report
        COMMAND
          "${OPENCPPCOVERAGE_EXECUTABLE}" --quiet --sources "${DOCK_SOURCE_DIR_NATIVE}" --export_type
          "cobertura:${DOCK_COVERAGE_OUTPUT_DIR}/coverage.cobertura.xml" --
          "$<TARGET_FILE:DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit>"
      )
      set_tests_properties(
        DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit.Coverage.Report
        PROPERTIES ENVIRONMENT "QT_QPA_PLATFORM=minimal"
      )
    else()
      message(
        WARNING
        "OpenCppCoverage not found — DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit will run without a coverage report. Install it (e.g. 'choco install opencppcoverage' or https://github.com/OpenCppCoverage/OpenCppCoverage/releases) and ensure it's on PATH."
      )
    endif()
  endif()
endif()

gtest_discover_tests(DynamiteAndy.DynamiteToolbar.Dock.Tests.Unit DISCOVERY_MODE PRE_TEST PROPERTIES ${DOCK_TEST_PROPERTIES})
