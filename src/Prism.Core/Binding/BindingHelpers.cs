using System.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal static class BindingHelpers
{
    extension(SyntaxToken token)
    {
        public string IdentifierName
        {
            get
            {
                Debug.Assert(token.Kind == SyntaxKind.IdentifierToken);
                return token.GetValue<IdentifierData>().Value;
            }
        }
    }

    extension(NameSyntax syntax)
    {
        public string UnqualifiedName
        {
            get
            {
                return syntax switch
                {
                    QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax
                        .Right
                        .UnqualifiedName,
                    SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.UnqualifiedName,
                    _ => throw new ArgumentOutOfRangeException(nameof(syntax)),
                };
            }
        }
    }

    extension(SimpleNameSyntax syntax)
    {
        public string UnqualifiedName => syntax.Identifier.IdentifierName;
    }
}
