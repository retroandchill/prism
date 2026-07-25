/**
 * @file helpers.ixx
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */
export module prism.core:syntax.green.helpers;

import :syntax.green.node;
import :syntax.green.token;

namespace prism
{

    template <std::derived_from<GreenNode> T>
    void replace_last_token(T &node, GreenPtr<GreenToken> token)
    {
    }
} // namespace prism
