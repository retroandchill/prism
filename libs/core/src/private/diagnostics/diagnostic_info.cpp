/**
 * @file diagnostic_info.cpp
 * @author Francesco Corso
 * @date 7/10/2026
 * @brief
 */
module;

#ifdef __JETBRAINS_IDE__
#include <mutex>
#endif

module prism.core:diagnostics.diagnostic_info.impl;

import :diagnostics.diagnostic_info;

namespace prism
{

    std::string_view DiagnosticInfo::message() const
    {
        std::call_once(message_once_, [this] { message_ = format_message_(descriptor_->message_format()); });

        return *message_;
    }
} // namespace prism
