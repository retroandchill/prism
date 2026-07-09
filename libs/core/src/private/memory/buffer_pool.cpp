/**
 * @file buffer_pool.cpp
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
module prism.core:memory.buffer_pool.impl;

import :memory.buffer_pool;

namespace prism
{
    BufferPool &BufferPool::shared()
    {
        thread_local BufferPool instance;
        return instance;
    }
} // namespace prism
