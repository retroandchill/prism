/**
 * @file per_project_boilerplate.hpp
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
#pragma once

#define PRISM_VISUALIZER_USE_AND_RETAIN

#define PRISM_VISUALIZER_HELPERS                                                                                       \
    PRISM_VISUALIZER_USE_AND_RETAIN auto *debug_name_table = prism::debug_get_name_memory();

#define PRISM_PER_PROJECT_BOILERPLATE PRISM_VISUALIZER_HELPERS
