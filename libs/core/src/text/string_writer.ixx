/**
 * @file string_writer.ixx
 * @author Francesco Corso
 * @date 7/25/2026
 * @brief
 */

export module prism.core:text.string_writer;

import :text.writer;
import :type_traits.basic;

namespace prism
{
    export template <SimpleAllocator Allocator>
    class BasicStringWriter final : public TextWriter
    {
      public:
        using BufferType = std::basic_string<char, std::char_traits<char>, Allocator>;

        explicit constexpr BasicStringWriter(BufferType &buffer) : buffer_{buffer}
        {
        }

        constexpr void write(std::string_view str) override
        {
            buffer_.append(str);
        }

      private:
        BufferType &buffer_;
    };

    export template <SimpleAllocator Allocator>
    BasicStringWriter(std::basic_string<char, std::char_traits<char>, Allocator> &buffer)
        -> BasicStringWriter<Allocator>;

    export using StringWriter = BasicStringWriter<std::allocator<char>>;

    namespace pmr
    {
        export using StringWriter = BasicStringWriter<std::pmr::polymorphic_allocator<char>>;
    }
} // namespace prism
