# CMake macOS helper functions module

include_guard(GLOBAL)

include(helpers_common)

# set_target_properties_obs: Set target properties for use in obs-studio
function(set_target_properties_plugin target)
  set(options "")
  set(oneValueArgs "")
  set(multiValueArgs PROPERTIES)
  cmake_parse_arguments(PARSE_ARGV 0 _STPO "${options}" "${oneValueArgs}" "${multiValueArgs}")

  message(DEBUG "Setting additional properties for target ${target}...")

  while(_STPO_PROPERTIES)
    list(POP_FRONT _STPO_PROPERTIES key value)
    set_property(TARGET ${target} PROPERTY ${key} "${value}")
  endwhile()

  string(TIMESTAMP CURRENT_YEAR "%Y")
  set_target_properties(
    ${target}
    PROPERTIES
      # BUNDLE TRUE is intentionally absent: it only applies to MODULE/executable
      # targets, not SHARED libraries. The plugin bundle is assembled at install
      # time (Contents/MacOS/ gets both the C++ dylib and the NativeAOT dylib).
      # The XCODE_ATTRIBUTE_* entries below still embed Info.plist metadata into
      # the dylib's __TEXT,__info_plist section, which is valid for code signing.
      XCODE_ATTRIBUTE_PRODUCT_NAME $<TARGET_FILE_BASE_NAME:${target}>
      XCODE_ATTRIBUTE_PRODUCT_BUNDLE_IDENTIFIER ${MACOS_BUNDLEID}
      XCODE_ATTRIBUTE_CURRENT_PROJECT_VERSION ${PLUGIN_BUILD_NUMBER}
      XCODE_ATTRIBUTE_MARKETING_VERSION ${PLUGIN_VERSION}
      XCODE_ATTRIBUTE_GENERATE_INFOPLIST_FILE YES
      XCODE_ATTRIBUTE_INFOPLIST_FILE ""
      XCODE_ATTRIBUTE_INFOPLIST_KEY_CFBundleDisplayName ${target}
      XCODE_ATTRIBUTE_INFOPLIST_KEY_NSHumanReadableCopyright "(c) ${CURRENT_YEAR} ${PLUGIN_AUTHOR}"
      XCODE_ATTRIBUTE_INSTALL_PATH "$(USER_LIBRARY_DIR)/Application Support/obs-studio/plugins"
      # Remove the default 'lib' prefix so the output matches what NativeLibraryLoader expects:
      # dynamiteandy.dynamite-toolbar.dock.dylib  (not libdynamiteandy.dynamite-toolbar.dock.dylib)
      PREFIX ""
  )

  if(EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/cmake/macos/entitlements.plist")
    set_target_properties(
      ${target}
      PROPERTIES XCODE_ATTRIBUTE_CODE_SIGN_ENTITLEMENTS "${CMAKE_CURRENT_SOURCE_DIR}/cmake/macos/entitlements.plist"
    )
  endif()

  if(TARGET plugin-support)
    target_link_libraries(${target} PRIVATE plugin-support)
  endif()

  target_install_resources(${target})

  # The Xcode generator only runs a link step (and produces a dylib) when the
  # target has at least one source file to compile. A SHARED target with no
  # direct sources — only an OBJECT library via target_link_libraries — is
  # treated by xcodebuild as script-only: no Ld step runs, no dylib is produced,
  # and the PostBuild copy then fails. Generate a minimal stub so xcodebuild
  # has a real translation unit to compile and link.
  #
  # This is macOS/Xcode-specific. Ninja (Linux) and MSBuild (Windows) handle
  # OBJECT library linking without requiring a direct source file.
  set(_xcode_link_stub "${CMAKE_CURRENT_BINARY_DIR}/${target}_xcode_stub.cpp")
  file(WRITE "${_xcode_link_stub}" "// macOS Xcode stub — ensures the Xcode generator produces a dylib link step.\n")
  target_sources(${target} PRIVATE "${_xcode_link_stub}")

  # The target is a SHARED dylib — no bundle directory is created at build time.
  # Construct the .plugin layout in rundir manually so the plugin can be tested
  # locally without a full cmake --install. Mirrors the install layout defined
  # in src/DynamiteAndy.DynamiteToolbar.Dock/CMakeLists.txt.
  add_custom_command(
    TARGET ${target}
    POST_BUILD
    COMMAND
      "${CMAKE_COMMAND}" -E make_directory
      "${CMAKE_CURRENT_BINARY_DIR}/rundir/$<CONFIG>/$<TARGET_FILE_BASE_NAME:${target}>.plugin/Contents/MacOS"
    COMMAND
      "${CMAKE_COMMAND}" -E copy_if_different "$<TARGET_FILE:${target}>"
      "${CMAKE_CURRENT_BINARY_DIR}/rundir/$<CONFIG>/$<TARGET_FILE_BASE_NAME:${target}>.plugin/Contents/MacOS/"
    COMMENT "Copy ${target} to rundir"
    VERBATIM
  )

  get_target_property(target_sources ${target} SOURCES)
  set(target_ui_files ${target_sources})
  list(FILTER target_ui_files INCLUDE REGEX ".+\\.(ui|qrc)")
  source_group(TREE "${CMAKE_CURRENT_SOURCE_DIR}" PREFIX "UI Files" FILES ${target_ui_files})

  # The platform-specific install for the dylib itself is handled in
  # src/DynamiteAndy.DynamiteToolbar.Dock/CMakeLists.txt. Only the dSYM is
  # installed here. For a SHARED dylib the dSYM sits next to the binary,
  # named after OUTPUT_NAME (e.g. dynamiteandy.dynamite-toolbar.dock.dylib.dSYM) —
  # NOT after the assembled .plugin bundle. package-macos archives debug
  # symbols as "<product_name>.plugin.dSYM" (a sibling of the .plugin bundle
  # itself), so the trailing slash on the source path here copies the dSYM's
  # *contents* into a destination directory renamed to match that convention,
  # rather than nesting it under the dylib's own OUTPUT_NAME.
  install(
    DIRECTORY "$<TARGET_FILE:${target}>.dSYM/"
    CONFIGURATIONS Release
    DESTINATION "${CMAKE_PROJECT_NAME}.plugin.dSYM"
    OPTIONAL
  )

  configure_file(
    "${CMAKE_SOURCE_DIR}/cmake/macos/resources/distribution.in"
    "${CMAKE_CURRENT_BINARY_DIR}/distribution"
    @ONLY
  )
  configure_file(
    "${CMAKE_SOURCE_DIR}/cmake/macos/resources/create-package.cmake.in"
    "${CMAKE_CURRENT_BINARY_DIR}/create-package.cmake"
    @ONLY
  )
  install(SCRIPT "${CMAKE_CURRENT_BINARY_DIR}/create-package.cmake")
endfunction()

# target_install_resources: Helper function to add resources into bundle
function(target_install_resources target)
  message(DEBUG "Installing resources for target ${target}...")
  if(EXISTS "${CMAKE_SOURCE_DIR}/data")
    file(GLOB_RECURSE data_files "${CMAKE_SOURCE_DIR}/data/*")
    foreach(data_file IN LISTS data_files)
      cmake_path(RELATIVE_PATH data_file BASE_DIRECTORY "${CMAKE_SOURCE_DIR}/data/" OUTPUT_VARIABLE relative_path)
      cmake_path(GET relative_path PARENT_PATH relative_path)
      target_sources(${target} PRIVATE "${data_file}")
      set_property(SOURCE "${data_file}" PROPERTY MACOSX_PACKAGE_LOCATION "Resources/data/${relative_path}")
      source_group("Resources/${relative_path}" FILES "${data_file}")
    endforeach()
    # Explicit install rule for the manually-assembled .plugin bundle
    # (MACOSX_PACKAGE_LOCATION only works for Xcode-managed bundles, not SHARED targets)
    install(
      DIRECTORY "${CMAKE_SOURCE_DIR}/data/"
      DESTINATION "${CMAKE_PROJECT_NAME}.plugin/Contents/Resources/data"
      USE_SOURCE_PERMISSIONS
    )
  endif()
endfunction()

# target_add_resource: Helper function to add a specific resource to a bundle
function(target_add_resource target resource)
  message(DEBUG "Add resource ${resource} to target ${target} at destination ${destination}...")
  target_sources(${target} PRIVATE "${resource}")
  set_property(SOURCE "${resource}" PROPERTY MACOSX_PACKAGE_LOCATION Resources)
  source_group("Resources" FILES "${resource}")
endfunction()
