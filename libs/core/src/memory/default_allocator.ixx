/**
 * @file default_allocator.ixx
 * @author Francesco Corso
 * @date 8/15/2026
 * @brief
 */
export module prism.core:memory.default_allocator;

import std;

namespace prism
{

    export template <typename T>
    using DefaultAllocator = std::allocator<T>;

}
