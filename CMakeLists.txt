cmake_minimum_required(VERSION 4.2)

if(CMAKE_VERSION VERSION_GREATER_EQUAL "4.4.0")
    set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
            "f35a9ac6-8463-4d38-8eec-5d6008153e7d"
    )
elseif(CMAKE_VERSION VERSION_GREATER_EQUAL "4.3.0")
    set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
            "451f2fe2-a8a2-47c3-bc32-94786d8fc91b"
    )
elseif(CMAKE_VERSION VERSION_GREATER_EQUAL "3.30.0")
    if(CMAKE_VERSION VERSION_LESS "3.31.8")
        set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
                "0e5b6991-d74f-4b3d-a41c-cf096e0b2508"
        )
    elseif(CMAKE_VERSION VERSION_LESS "4.0.0")
        set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
                "d0edc3af-4c50-42ea-a356-e2862fe7a444"
        )
    elseif(CMAKE_VERSION VERSION_LESS "4.0.3")
        set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
                "a9e1cf81-9932-4810-974b-6eccaf14e457"
        )
    elseif(CMAKE_VERSION VERSION_LESS "4.3.0")
        set(CMAKE_EXPERIMENTAL_CXX_IMPORT_STD
                "d0edc3af-4c50-42ea-a356-e2862fe7a444"
        )
    endif()
endif()

option(PRISM_BUILD_SHARED "Build as a shared library" OFF)

option(PRISM_BUILD_TESTS "Build tests for the library" ON)

if (PRISM_BUILD_SHARED)
    set(BUILD_SHARED_LIBS ON)
else ()
    set(BUILD_SHARED_LIBS OFF)

    set(VCPKG_CRT_LINKAGE static)
    set(VCPKG_LIBRARY_LINKAGE static)
endif ()

set(VCPKG_DEPS_DIR ${CMAKE_BINARY_DIR}/vcpkg_installed/${VCPKG_TARGET_TRIPLET})
set(VCPKG_BIN_DIR ${VCPKG_DEPS_DIR}/bin)

if (PRISM_BUILD_SHARED)
    add_compile_definitions(PRISM_SHARED)
endif()

project(prism LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_MODULE_STD ON)
if (MSVC)
    add_compile_options(/Zc:preprocessor)
endif()

set(TOOLS_DIR ${CMAKE_BINARY_DIR}/tools)

include(cmake/Boilerplate.cmake)

if (PRISM_BUILD_TESTS)
    include(CTest)

    find_package(Catch2 CONFIG REQUIRED)
    list(APPEND CMAKE_MODULE_PATH ${catch2_SOURCE_DIR}/extra)
    include(Catch)

    enable_testing()
endif()

add_subdirectory(third_party)
add_subdirectory(libs)
