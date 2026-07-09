function(prism_add_boilerplate TARGET_NAME)
    get_target_property(TARGET_TYPE ${TARGET_NAME} TYPE)

    if(TARGET_TYPE STREQUAL "EXECUTABLE" OR TARGET_TYPE STREQUAL "SHARED_LIBRARY")
        set(BOILERPLATE_FILE "${CMAKE_CURRENT_BINARY_DIR}/${TARGET_NAME}_boilerplate.cpp")
        configure_file(
                "${CMAKE_SOURCE_DIR}/templates/per_project_boilerplate.cpp.in"
                "${BOILERPLATE_FILE}"
                @ONLY
        )
        target_sources(${TARGET_NAME} PRIVATE "${BOILERPLATE_FILE}")
    endif()
endfunction()
