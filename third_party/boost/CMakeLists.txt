# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(BOOST_MODULES boost.ixx
        int128.ixx)

add_library(boost_cppm OBJECT ${BOOST_MODULES})

target_sources(boost_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        ${BOOST_MODULES})


find_package(boost_container CONFIG REQUIRED)
find_package(boost_multiprecision CONFIG REQUIRED)
find_package(boost_unordered CONFIG REQUIRED)
target_link_libraries(boost_cppm PUBLIC Boost::container Boost::multiprecision  Boost::unordered)

find_path(INT128_INCLUDE_DIRS "boost/int128.hpp")
target_include_directories(boost_cppm PRIVATE ${INT128_INCLUDE_DIRS})

add_library(Boost::cppm ALIAS boost_cppm)
