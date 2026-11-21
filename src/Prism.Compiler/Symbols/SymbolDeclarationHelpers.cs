// @file SymbolDeclarationHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Compiler.Symbols;

internal static partial class SymbolDeclarationHelpers
{
    public static DeclarationModifiers GetDefaultMemberVisibility(
        this MemberContainerSymbol containingSymbol
    )
    {
        return containingSymbol switch
        {
            NamespaceSymbol => DeclarationModifiers.Internal,
            TypeSymbol => DeclarationModifiers.Private,
        };
    }

    extension(DeclarationModifiers)
    {
        public static DeclarationModifiers MakeModifiers(
            MemberContainerSymbol containingSymbol,
            SyntaxTokenList modifiers
        )
        {
            var defaultAccess = containingSymbol.GetDefaultMemberVisibility();
            return DeclarationModifiers.MakeModifiers(defaultAccess, modifiers);
        }

        public static DeclarationModifiers MakeModifiers(
            DeclarationModifiers defaultAccess,
            SyntaxTokenList modifiers
        )
        {
            var result = DeclarationModifiers.None;
            var accessModifierSeen = false;
            foreach (var modifier in modifiers)
            {
                switch (modifier.Kind)
                {
                    case SyntaxKind.ExternKeyword:
                        result |= DeclarationModifiers.Extern;
                        break;
                    case SyntaxKind.PublicKeyword:
                        accessModifierSeen = true;
                        result |= DeclarationModifiers.Public;
                        break;
                    case SyntaxKind.InternalKeyword:
                        accessModifierSeen = true;
                        if (result.HasFlag(DeclarationModifiers.Protected))
                        {
                            result &= ~DeclarationModifiers.Protected;
                            result |= DeclarationModifiers.ProtectedInternal;
                        }
                        else
                        {
                            result |= DeclarationModifiers.Internal;
                        }

                        break;
                    case SyntaxKind.ProtectedKeyword:
                        accessModifierSeen = true;
                        if (result.HasFlag(DeclarationModifiers.Internal))
                        {
                            result &= ~DeclarationModifiers.Internal;
                            result |= DeclarationModifiers.ProtectedInternal;
                        }
                        else if (result.HasFlag(DeclarationModifiers.Private))
                        {
                            result &= ~DeclarationModifiers.Private;
                            result |= DeclarationModifiers.PrivateProtected;
                        }
                        else
                        {
                            result |= DeclarationModifiers.Protected;
                        }

                        break;
                    case SyntaxKind.PrivateKeyword:
                        accessModifierSeen = true;
                        if (result.HasFlag(DeclarationModifiers.Protected))
                        {
                            result &= ~DeclarationModifiers.Protected;
                            result |= DeclarationModifiers.PrivateProtected;
                        }
                        else
                        {
                            result |= DeclarationModifiers.Private;
                        }
                        break;
                    case SyntaxKind.FileKeyword:
                        accessModifierSeen = true;
                        result |= DeclarationModifiers.File;
                        break;
                    case SyntaxKind.StaticKeyword:
                        result |= DeclarationModifiers.Static;
                        break;
                }
            }

            if (!accessModifierSeen)
            {
                result |= defaultAccess;
            }

            return result;
        }
    }
}
