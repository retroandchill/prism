set(PRISM_HEADERS )

set(PRISM_SOURCES )

set(PRISM_PUBLIC_MODULES )



add_library(prism_core ${PRISM_HEADERS} ${PRISM_SOURCES} ${PRISM_PUBLIC_MODULES})

target_sources(prism_core PUBLIC
        FILE_SET prism_core_public
        TYPE CXX_MODULES
        FILES
        ${PRISM_PUBLIC_MODULES})