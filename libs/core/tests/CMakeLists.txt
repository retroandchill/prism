find_package(Catch2 CONFIG REQUIRED)

add_executable(prism_core_tests
        lexer_test.cpp)

target_link_libraries(prism_core_tests PRIVATE Catch2::Catch2WithMain prism_core)

list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extra)
include(Catch)
catch_discover_tests(prism_core_tests)
