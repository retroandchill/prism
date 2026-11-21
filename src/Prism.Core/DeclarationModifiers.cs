// @file DeclarationModifiers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism;

[Flags]
internal enum DeclarationModifiers : uint
{
    None = 0,
    Extern = 1 << 0,

    Public = 1 << 1,
    Internal = 1 << 2,
    Protected = 1 << 3,
    ProtectedInternal = 1 << 4,
    Private = 1 << 5,
    PrivateProtected = 1 << 6,
    File = 1 << 7,

    Static = 1 << 8,
    Mutable = 1 << 9,

    AccessibilityMask =
        Public | Internal | Protected | ProtectedInternal | Private | PrivateProtected | File,
}

internal static class DeclarationModifiersExtensions
{
    extension(DeclaredVisibility)
    {
        internal static DeclaredVisibility FromDeclarationModifiers(DeclarationModifiers modifiers)
        {
            return (modifiers & DeclarationModifiers.AccessibilityMask) switch
            {
                DeclarationModifiers.Public => DeclaredVisibility.Public,
                DeclarationModifiers.Internal => DeclaredVisibility.Internal,
                DeclarationModifiers.Protected => DeclaredVisibility.Protected,
                DeclarationModifiers.ProtectedInternal => DeclaredVisibility.ProtectedOrInternal,
                DeclarationModifiers.PrivateProtected => DeclaredVisibility.ProtectedAndInternal,
                DeclarationModifiers.Private => DeclaredVisibility.Private,
                DeclarationModifiers.File => DeclaredVisibility.File,
                // Public is a fallback for when a malformed modifier combination is encountered (ex. public private)
                _ => DeclaredVisibility.Public,
            };
        }
    }
}
