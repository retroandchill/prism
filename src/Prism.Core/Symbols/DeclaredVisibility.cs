namespace Prism.Core.Symbols;

public enum DeclaredVisibility
{
    NotApplicable,
    Public,
    Internal,
    Protected,
    ProtectedOrInternal,
    ProtectedAndInternal,
    Private,
    File,
}

public static class DeclaredVisibilityExtensions
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
