namespace Prism.Core.Syntax;

public partial class VariableDeclarationSyntax
{
    public abstract bool IsConst { get; }
}

public partial class GlobalVariableDeclarationSyntax
{
    public override bool IsConst => Keyword.Kind == SyntaxKind.ConstKeyword;
}

public partial class LocalVariableDeclarationSyntax
{
    public override bool IsConst => Keyword.Kind == SyntaxKind.ConstKeyword;
}

public partial class FieldDeclarationSyntax
{
    public override bool IsConst => ConstKeyword is not null;
}
