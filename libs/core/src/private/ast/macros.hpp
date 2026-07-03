/**
 * @file macros.hpp
 * @author Francesco Corso
 * @date 7/2/2026
 * @brief
 */
#pragma once

#define AST_NODE_BOILERPLATE(name)                                                                                     \
    ~name() = default;                                                                                                 \
                                                                                                                       \
  public:                                                                                                              \
    name(const name &) = default;                                                                                      \
    name(name &&) = default;                                                                                           \
    name &operator=(const name &) = default;                                                                           \
    name &operator=(name &&) = default;

#define ABSTRACT_AST_NODE(name)

#define VALUE_PROPERTY(type, name)                                                                                     \
  private:                                                                                                             \
    type name##_;                                                                                                      \
                                                                                                                       \
  public:                                                                                                              \
    [[nodiscard]] constexpr type name() const noexcept                                                                 \
    {                                                                                                                  \
        return name##_;                                                                                                \
    }

#define OWNED_REF_PROPERTY(type, name)                                                                                 \
  private:                                                                                                             \
    type name##_;                                                                                                      \
                                                                                                                       \
  public:                                                                                                              \
    [[nodiscard]] constexpr const type &name() const noexcept                                                          \
    {                                                                                                                  \
        return name##_;                                                                                                \
    }

#define UNOWNED_REF_PROPERTY(type, name)                                                                               \
  private:                                                                                                             \
    std::reference_wrapper<const type> name##_;                                                                        \
                                                                                                                       \
  public:                                                                                                              \
    [[nodiscard]] constexpr const type &name() const noexcept                                                          \
    {                                                                                                                  \
        return name##_;                                                                                                \
    }
