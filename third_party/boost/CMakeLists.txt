set(BOOST_MODULES flyweight.ixx)

add_library(boost_cppm OBJECT ${BOOST_MODULES})


find_package(Boost REQUIRED COMPONENTS flyweight)
target_link_libraries(boost_cppm PUBLIC Boost::flyweight)

target_sources(boost_cppm PUBLIC
        FILE_SET CXX_MODULES
        FILES
        ${BOOST_MODULES})

add_library(Boost::cppm ALIAS boost_cppm)
