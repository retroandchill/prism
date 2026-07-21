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

    export class InvalidStateException : public std::logic_error
    {
      public:
        using std::logic_error::logic_error;
    };

    export class NotImplementedException : public std::exception
    {
      public:
        constexpr NotImplementedException() : std::exception{"Not implemented"}
        {
        }
    };
} // namespace prism
