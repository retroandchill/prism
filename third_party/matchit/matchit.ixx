/**
 * @file matchit.ixx
 * @author Francesco Corso
 * @date 7/1/2026
 * @brief
 */
module;

#include <matchit.h>

export module matchit;

namespace matchit
{
    export using impl::match;
    export using impl::expr;
    export using impl::_;
    export using impl::and_;
    export using impl::app;
    export using impl::ds;
    export using impl::Id;
    export using impl::meet;
    export using impl::not_;
    export using impl::ooo;
    export using impl::or_;
    export using impl::pattern;
    export using impl::Subrange;
    export using impl::SubrangeT;
    export using impl::when;
    export using impl::as;
    export using impl::asDsVia;
    export using impl::dsVia;
    export using impl::matched;
    export using impl::none;
    export using impl::some;
} // namespace matchit
