// Generated file, do not edit
namespace Prism.Core.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnexpectedToken = new()
    {
        Id = DiagnosticCode.UnexpectedToken.ToStringFast(),
        Title = "Unexpected token",
        MessageFormat = "Unexpected token {Token}",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnexpectedEOF = new()
    {
        Id = DiagnosticCode.UnexpectedEOF.ToStringFast(),
        Title = "Unexpected EOF",
        MessageFormat = "Unexpected end of file",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnexpectedEscape = new()
    {
        Id = DiagnosticCode.UnexpectedEscape.ToStringFast(),
        Title = "Unexpected escape",
        MessageFormat = "Unexpected escape sequence: '{Sequence}'",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnterminatedStringLiteral = new()
    {
        Id = DiagnosticCode.UnterminatedStringLiteral.ToStringFast(),
        Title = "Unterminated string literal",
        MessageFormat = "Unterminated string literal",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnterminatedCharacterLiteral = new()
    {
        Id = DiagnosticCode.UnterminatedCharacterLiteral.ToStringFast(),
        Title = "Unterminated character literal",
        MessageFormat = "Unterminated character literal",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnterminatedBlockComment = new()
    {
        Id = DiagnosticCode.UnterminatedBlockComment.ToStringFast(),
        Title = "Unterminated block comment",
        MessageFormat = "Unterminated block comment",
        Category = "Syntax",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnresolvedSymbol = new()
    {
        Id = DiagnosticCode.UnresolvedSymbol.ToStringFast(),
        Title = "Unresolved symbol",
        MessageFormat = "Unresolved symbol {Symbol}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidSymbol = new()
    {
        Id = DiagnosticCode.InvalidSymbol.ToStringFast(),
        Title = "Invalid symbol",
        MessageFormat = "Unexpected symbol {Symbol}, expected a {Expected}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor ExpectedTypeSpecifier = new()
    {
        Id = DiagnosticCode.ExpectedTypeSpecifier.ToStringFast(),
        Title = "Expected type specifier",
        MessageFormat = "Type specifier expected",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor AmbiguousSymbol = new()
    {
        Id = DiagnosticCode.AmbiguousSymbol.ToStringFast(),
        Title = "Ambiguous symbol",
        MessageFormat = "Ambiguous symbol reference {Symbol}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor SymbolInaccessible = new()
    {
        Id = DiagnosticCode.SymbolInaccessible.ToStringFast(),
        Title = "Symbol inaccessible",
        MessageFormat = "Symbol {Symbol} is inaccessible",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoConversion = new()
    {
        Id = DiagnosticCode.NoConversion.ToStringFast(),
        Title = "No conversion",
        MessageFormat = "No conversion exists between type {FromType} and type {ToType}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor ConversionIsExplicit = new()
    {
        Id = DiagnosticCode.ConversionIsExplicit.ToStringFast(),
        Title = "Conversion is explicit",
        MessageFormat = "Conversion from {FromType} to type {ToType} is explicit",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor MultipleFileScopedNamespaces = new()
    {
        Id = DiagnosticCode.MultipleFileScopedNamespaces.ToStringFast(),
        Title = "Multiple file scoped namespaces",
        MessageFormat = "Multiple file-scoped namespaces are not allowed",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor FileScopedAndNormalNamespace = new()
    {
        Id = DiagnosticCode.FileScopedAndNormalNamespace.ToStringFast(),
        Title = "File scoped and normal namespace",
        MessageFormat = "Cannot mix file-scoped and block-scoped namespaces",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor FileScopedNamespaceNotBeforeAllMembers = new()
    {
        Id = DiagnosticCode.FileScopedNamespaceNotBeforeAllMembers.ToStringFast(),
        Title = "File scoped namespace not before all members",
        MessageFormat = "File-scoped namespace must be declared before all other members",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor BadModifiersOnNamespace = new()
    {
        Id = DiagnosticCode.BadModifiersOnNamespace.ToStringFast(),
        Title = "Bad modifiers on namespace",
        MessageFormat = "Namespaces cannot have modifiers",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor LiteralValueTooBig = new()
    {
        Id = DiagnosticCode.LiteralValueTooBig.ToStringFast(),
        Title = "Literal value too big",
        MessageFormat = "Literal value is too big for its type",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnaryOperatorUndefined = new()
    {
        Id = DiagnosticCode.UnaryOperatorUndefined.ToStringFast(),
        Title = "Unary operator undefined",
        MessageFormat = "Unary operator not defined for type {Type}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotAssignExpression = new()
    {
        Id = DiagnosticCode.CannotAssignExpression.ToStringFast(),
        Title = "Cannot assign expression",
        MessageFormat = "Cannot assign value to expression",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoCompoundAssignmentOperator = new()
    {
        Id = DiagnosticCode.NoCompoundAssignmentOperator.ToStringFast(),
        Title = "No compound assignment operator",
        MessageFormat = "No compound assignment operator defined for type {Type}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor BinaryOperatorUndefined = new()
    {
        Id = DiagnosticCode.BinaryOperatorUndefined.ToStringFast(),
        Title = "Binary operator undefined",
        MessageFormat = "Unary operator not defined for types {Type1} and {Type2}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoOverloadMatchingArgCount = new()
    {
        Id = DiagnosticCode.NoOverloadMatchingArgCount.ToStringFast(),
        Title = "No overload matching arg count",
        MessageFormat = "No overload that accepts {ArgCount} arguments",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoOverloadForArgTypes = new()
    {
        Id = DiagnosticCode.NoOverloadForArgTypes.ToStringFast(),
        Title = "No overload for arg types",
        MessageFormat = "No overload for argument types {ArgTypes}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoCallOperatorDefined = new()
    {
        Id = DiagnosticCode.NoCallOperatorDefined.ToStringFast(),
        Title = "No call operator defined",
        MessageFormat = "Type {Type} is not callable",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoEntryPointDefined = new()
    {
        Id = DiagnosticCode.NoEntryPointDefined.ToStringFast(),
        Title = "No entry point defined",
        MessageFormat = "No entry point defined",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor AmbiguousEntryPoint = new()
    {
        Id = DiagnosticCode.AmbiguousEntryPoint.ToStringFast(),
        Title = "Ambiguous entry point",
        MessageFormat = "Ambiguous entry point, could be any of: {Names}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidEntryPointReturnType = new()
    {
        Id = DiagnosticCode.InvalidEntryPointReturnType.ToStringFast(),
        Title = "Invalid entry point return type",
        MessageFormat = "Return type {Type} will main function {Name} not usable as an entry point",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Warning,
    };

    public static readonly DiagnosticDescriptor InvalidEntryPointParameters = new()
    {
        Id = DiagnosticCode.InvalidEntryPointParameters.ToStringFast(),
        Title = "Invalid entry point parameters",
        MessageFormat =
            "Parameters ({Types}) will main function {Name} not usable as an entry point",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Warning,
    };

    public static DiagnosticDescriptor? Find(DiagnosticCode code)
    {
        return code switch
        {
            DiagnosticCode.UnexpectedToken => UnexpectedToken,
            DiagnosticCode.UnexpectedEOF => UnexpectedEOF,
            DiagnosticCode.UnexpectedEscape => UnexpectedEscape,
            DiagnosticCode.UnterminatedStringLiteral => UnterminatedStringLiteral,
            DiagnosticCode.UnterminatedCharacterLiteral => UnterminatedCharacterLiteral,
            DiagnosticCode.UnterminatedBlockComment => UnterminatedBlockComment,
            DiagnosticCode.UnresolvedSymbol => UnresolvedSymbol,
            DiagnosticCode.InvalidSymbol => InvalidSymbol,
            DiagnosticCode.ExpectedTypeSpecifier => ExpectedTypeSpecifier,
            DiagnosticCode.AmbiguousSymbol => AmbiguousSymbol,
            DiagnosticCode.SymbolInaccessible => SymbolInaccessible,
            DiagnosticCode.NoConversion => NoConversion,
            DiagnosticCode.ConversionIsExplicit => ConversionIsExplicit,
            DiagnosticCode.MultipleFileScopedNamespaces => MultipleFileScopedNamespaces,
            DiagnosticCode.FileScopedAndNormalNamespace => FileScopedAndNormalNamespace,
            DiagnosticCode.FileScopedNamespaceNotBeforeAllMembers =>
                FileScopedNamespaceNotBeforeAllMembers,
            DiagnosticCode.BadModifiersOnNamespace => BadModifiersOnNamespace,
            DiagnosticCode.LiteralValueTooBig => LiteralValueTooBig,
            DiagnosticCode.UnaryOperatorUndefined => UnaryOperatorUndefined,
            DiagnosticCode.CannotAssignExpression => CannotAssignExpression,
            DiagnosticCode.NoCompoundAssignmentOperator => NoCompoundAssignmentOperator,
            DiagnosticCode.BinaryOperatorUndefined => BinaryOperatorUndefined,
            DiagnosticCode.NoOverloadMatchingArgCount => NoOverloadMatchingArgCount,
            DiagnosticCode.NoOverloadForArgTypes => NoOverloadForArgTypes,
            DiagnosticCode.NoCallOperatorDefined => NoCallOperatorDefined,
            DiagnosticCode.NoEntryPointDefined => NoEntryPointDefined,
            DiagnosticCode.AmbiguousEntryPoint => AmbiguousEntryPoint,
            DiagnosticCode.InvalidEntryPointReturnType => InvalidEntryPointReturnType,
            DiagnosticCode.InvalidEntryPointParameters => InvalidEntryPointParameters,
            _ => null,
        };
    }
}
