add_executable(prism_core_tests
        lexer_test.cpp)

target_link_libraries(prism_core_tests PRIVATE Catch2::Catch2WithMain prism_core)


catch_discover_tests(prism_core_tests)
