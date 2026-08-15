set(PRISM_CORE_TESTS
        lazy_test.cpp
        immutable_array_test.cpp
        immutable_small_array_test.cpp
        immutable_hash_collections_test.cpp)

set(PRISM_CORE_INTERNAL_TESTS name_test.cpp
        source_file_test.cpp
        lexer_test.cpp
        parser_test.cpp
        symbol_creation_test.cpp)

add_executable(prism_core_tests ${PRISM_CORE_TESTS} ${PRISM_CORE_INTERNAL_TESTS})
prism_add_boilerplate(prism_core_tests)

target_link_libraries(prism_core_tests PRIVATE Catch2::Catch2WithMain prism_core)

target_sources(prism_core_tests PUBLIC
        FILE_SET prism_core_tests_private
        TYPE CXX_MODULES
        FILES
        ${PRISM_CORE_INTERNAL_TESTS})


catch_discover_tests(prism_core_tests)
