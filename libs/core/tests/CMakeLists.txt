add_executable(prism_core_tests
        name_test.cpp
        source_file_test.cpp
        lexer_test.cpp)
prism_add_boilerplate(prism_core_tests)

target_link_libraries(prism_core_tests PRIVATE Catch2::Catch2WithMain prism_core)


catch_discover_tests(prism_core_tests)
