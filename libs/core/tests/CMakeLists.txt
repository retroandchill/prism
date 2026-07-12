set(PRISM_CORE_TESTS name_test.cpp
        source_file_test.cpp
        lexer_test.cpp)

add_executable(prism_core_tests ${PRISM_CORE_TESTS})
prism_add_boilerplate(prism_core_tests)

target_link_libraries(prism_core_tests PRIVATE Catch2::Catch2WithMain prism_core)

target_sources(prism_core_tests PUBLIC
        FILE_SET prism_core_tests_private
        TYPE CXX_MODULES
        FILES
        ${PRISM_CORE_TESTS})


catch_discover_tests(prism_core_tests)
