/**
 * @file lifetime.cpp
 * @author Francesco Corso
 * @date 7/20/2026
 * @brief
 */
module prism.core:syntax.lifetime.impl;

import :syntax.lifetime;

namespace prism
{

    void SyntaxLifetime::reset() noexcept
    {
        std::scoped_lock lock{mutex_};
        allocator_.reset();
        roots_.clear();
    }
} // namespace prism
