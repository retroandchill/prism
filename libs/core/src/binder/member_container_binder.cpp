/**
 * @file member_container_binder.cpp
 * @author Francesco Corso
 * @date 8/17/2026
 * @brief
 */
module prism.core:binder.member_container_binder.impl;

import :binder.member_container_binder;

namespace prism
{

    MemberContainerBinder::MemberContainerBinder(const Binder &next, const MemberContainerSymbol &symbol)
        : Binder{next}, symbol_{symbol}
    {
    }
} // namespace prism
