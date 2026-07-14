# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
set(BOOST_MODULES boost.ixx)

add_library(boost_cppm OBJECT ${BOOST_MODULES})

target_sources(boost_cppm PUBLIC
        FILE_SET CXX_MODULES FILES
        ${BOOST_MODULES})


find_package(boost_container CONFIG REQUIRED)
find_package(boost_multiprecision CONFIG REQUIRED)
target_link_libraries(boost_cppm PUBLIC Boost::container Boost::multiprecision)

add_library(Boost::cppm ALIAS boost_cppm)
