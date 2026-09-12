// Generated file, do not edit
namespace Prism.Core.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnexpectedToken = new()
    {
        Id = DiagnosticCode.UnexpectedToken.ToStringFast(),
        Title = "Unexpected token",
        MessageFormat = "Unexpected token {0}",
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
        MessageFormat = "Unexpected escape sequence: '{0}'",
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
        MessageFormat = "Unresolved symbol {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidSymbol = new()
    {
        Id = DiagnosticCode.InvalidSymbol.ToStringFast(),
        Title = "Invalid symbol",
        MessageFormat = "Unexpected symbol {0}, expected a {1}",
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
        MessageFormat = "Ambiguous symbol reference {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor SymbolInaccessible = new()
    {
        Id = DiagnosticCode.SymbolInaccessible.ToStringFast(),
        Title = "Symbol inaccessible",
        MessageFormat = "Symbol {0} is inaccessible",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoConversion = new()
    {
        Id = DiagnosticCode.NoConversion.ToStringFast(),
        Title = "No conversion",
        MessageFormat = "No conversion exists between type {0} and type {1}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor ConversionIsExplicit = new()
    {
        Id = DiagnosticCode.ConversionIsExplicit.ToStringFast(),
        Title = "Conversion is explicit",
        MessageFormat = "Conversion from {0} to type {1} is explicit",
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
        MessageFormat = "Unary operator not defined for type {0}",
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
        MessageFormat = "No compound assignment operator defined for type {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor BinaryOperatorUndefined = new()
    {
        Id = DiagnosticCode.BinaryOperatorUndefined.ToStringFast(),
        Title = "Binary operator undefined",
        MessageFormat = "Unary operator not defined for types {0} and {1}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoOverloadMatchingArgCount = new()
    {
        Id = DiagnosticCode.NoOverloadMatchingArgCount.ToStringFast(),
        Title = "No overload matching arg count",
        MessageFormat = "No overload that accepts {0} arguments",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoOverloadForArgTypes = new()
    {
        Id = DiagnosticCode.NoOverloadForArgTypes.ToStringFast(),
        Title = "No overload for arg types",
        MessageFormat = "No overload for argument types {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor AmbiguousOverloadDefined = new()
    {
        Id = DiagnosticCode.AmbiguousOverloadDefined.ToStringFast(),
        Title = "Ambiguous overload defined",
        MessageFormat = "Ambiguous overload for argument types {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoCallOperatorDefined = new()
    {
        Id = DiagnosticCode.NoCallOperatorDefined.ToStringFast(),
        Title = "No call operator defined",
        MessageFormat = "Type {0} is not callable",
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
        MessageFormat = "Ambiguous entry point, could be any of: {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidEntryPointReturnType = new()
    {
        Id = DiagnosticCode.InvalidEntryPointReturnType.ToStringFast(),
        Title = "Invalid entry point return type",
        MessageFormat = "Return type {0} will main function {1} not usable as an entry point",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Warning,
    };

    public static readonly DiagnosticDescriptor InvalidEntryPointParameters = new()
    {
        Id = DiagnosticCode.InvalidEntryPointParameters.ToStringFast(),
        Title = "Invalid entry point parameters",
        MessageFormat = "Parameters ({0}) will main function {1} not usable as an entry point",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Warning,
    };

    public static readonly DiagnosticDescriptor InvalidUseOfLabel = new()
    {
        Id = DiagnosticCode.InvalidUseOfLabel.ToStringFast(),
        Title = "Invalid use of label",
        MessageFormat = "Labels may only be used on loops",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidUseOfBreak = new()
    {
        Id = DiagnosticCode.InvalidUseOfBreak.ToStringFast(),
        Title = "Invalid use of break",
        MessageFormat = "Break may only be used inside loops",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor InvalidUseOfContinue = new()
    {
        Id = DiagnosticCode.InvalidUseOfContinue.ToStringFast(),
        Title = "Invalid use of continue",
        MessageFormat = "Continue may only be used inside loops",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotTakeAddress = new()
    {
        Id = DiagnosticCode.CannotTakeAddress.ToStringFast(),
        Title = "Cannot take address",
        MessageFormat = "Cannot take address of a temporary",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotDereference = new()
    {
        Id = DiagnosticCode.CannotDereference.ToStringFast(),
        Title = "Cannot dereference",
        MessageFormat = "Cannot dereference a non-reference type",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotDereferenceVariableLength = new()
    {
        Id = DiagnosticCode.CannotDereferenceVariableLength.ToStringFast(),
        Title = "Cannot dereference variable length",
        MessageFormat = "Cannot dereference a variable length type {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NoIndexOperation = new()
    {
        Id = DiagnosticCode.NoIndexOperation.ToStringFast(),
        Title = "No index operation",
        MessageFormat = "Cannot perform index operation on {0}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor AllPathsMustReturnValue = new()
    {
        Id = DiagnosticCode.AllPathsMustReturnValue.ToStringFast(),
        Title = "All paths must return value",
        MessageFormat = "All control paths must return a value",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor StatementMustReturnValue = new()
    {
        Id = DiagnosticCode.StatementMustReturnValue.ToStringFast(),
        Title = "Statement must return value",
        MessageFormat = "Function must return a value",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotReturnValue = new()
    {
        Id = DiagnosticCode.CannotReturnValue.ToStringFast(),
        Title = "Cannot return value",
        MessageFormat = "Function does not return a value",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor UnreachableCode = new()
    {
        Id = DiagnosticCode.UnreachableCode.ToStringFast(),
        Title = "Unreachable code",
        MessageFormat = "Unreachable code",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotReassign = new()
    {
        Id = DiagnosticCode.CannotReassign.ToStringFast(),
        Title = "Cannot reassign",
        MessageFormat = "Variable {0} is not mutable, cannot be reassigned",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor ValueUninitialized = new()
    {
        Id = DiagnosticCode.ValueUninitialized.ToStringFast(),
        Title = "Value uninitialized",
        MessageFormat = "Variable {0} is uninitialized",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor NotInitializedOnAllPaths = new()
    {
        Id = DiagnosticCode.NotInitializedOnAllPaths.ToStringFast(),
        Title = "Not initialized on all paths",
        MessageFormat = "Variable {0} is not initialized on all paths",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor IncompatibleCollectionSizes = new()
    {
        Id = DiagnosticCode.IncompatibleCollectionSizes.ToStringFast(),
        Title = "Incompatible collection sizes",
        MessageFormat = "Cannot initialize an array of size {0} with a size of {1}",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor CannotInferType = new()
    {
        Id = DiagnosticCode.CannotInferType.ToStringFast(),
        Title = "Cannot infer type",
        MessageFormat = "Cannot infer type from expression",
        Category = "Semantic",
        DefaultSeverity = DiagnosticSeverity.Error,
    };

    public static readonly DiagnosticDescriptor ExternalDiagnostic = new()
    {
        Id = DiagnosticCode.ExternalDiagnostic.ToStringFast(),
        Title = "External diagnostic",
        MessageFormat = "An external diagnostic occurred: {0}",
        Category = "CodeGen",
        DefaultSeverity = DiagnosticSeverity.Info,
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
            DiagnosticCode.AmbiguousOverloadDefined => AmbiguousOverloadDefined,
            DiagnosticCode.NoCallOperatorDefined => NoCallOperatorDefined,
            DiagnosticCode.NoEntryPointDefined => NoEntryPointDefined,
            DiagnosticCode.AmbiguousEntryPoint => AmbiguousEntryPoint,
            DiagnosticCode.InvalidEntryPointReturnType => InvalidEntryPointReturnType,
            DiagnosticCode.InvalidEntryPointParameters => InvalidEntryPointParameters,
            DiagnosticCode.InvalidUseOfLabel => InvalidUseOfLabel,
            DiagnosticCode.InvalidUseOfBreak => InvalidUseOfBreak,
            DiagnosticCode.InvalidUseOfContinue => InvalidUseOfContinue,
            DiagnosticCode.CannotTakeAddress => CannotTakeAddress,
            DiagnosticCode.CannotDereference => CannotDereference,
            DiagnosticCode.CannotDereferenceVariableLength => CannotDereferenceVariableLength,
            DiagnosticCode.NoIndexOperation => NoIndexOperation,
            DiagnosticCode.AllPathsMustReturnValue => AllPathsMustReturnValue,
            DiagnosticCode.StatementMustReturnValue => StatementMustReturnValue,
            DiagnosticCode.CannotReturnValue => CannotReturnValue,
            DiagnosticCode.UnreachableCode => UnreachableCode,
            DiagnosticCode.CannotReassign => CannotReassign,
            DiagnosticCode.ValueUninitialized => ValueUninitialized,
            DiagnosticCode.NotInitializedOnAllPaths => NotInitializedOnAllPaths,
            DiagnosticCode.IncompatibleCollectionSizes => IncompatibleCollectionSizes,
            DiagnosticCode.CannotInferType => CannotInferType,
            DiagnosticCode.ExternalDiagnostic => ExternalDiagnostic,
            _ => null,
        };
    }
}
