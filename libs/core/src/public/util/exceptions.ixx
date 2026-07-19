/**
 * @file exceptions.ixx
 * @author Francesco Corso
 * @date 7/19/2026
 * @brief
 */
export module prism.core:util.exceptions;

import std;

namespace prism
{
    export class UnsupportedOperationException : public std::logic_error
    {
      public:
        constexpr UnsupportedOperationException() : std::logic_error{"Unsupported operation"}
        {
        }
    };
} // namespace prism
