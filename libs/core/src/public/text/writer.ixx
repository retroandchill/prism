/**
 * @file writer.ixx
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */

export module prism.core:text.writer;

import std;
import :text.name;

namespace prism
{
    export class TextWriter
    {
      public:
        virtual ~TextWriter() = default;

        virtual void write(std::string_view str) = 0;

        void write(char c)
        {
            write(std::string_view{&c, 1});
        }
    };
} // namespace prism
