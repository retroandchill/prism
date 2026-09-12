// Generated file, do not edit
using System.Runtime.Serialization;
using NetEscapades.EnumGenerators;

namespace Prism.Core.Diagnostics;

[EnumExtensions]
public enum DiagnosticCode : uint
{
    Unknown = 0,

    // -- Syntax (1000-1999) ---
    [EnumMember(Value = "E1000")]
    UnexpectedToken = 1000,

    [EnumMember(Value = "E1001")]
    UnexpectedEOF = 1001,

    [EnumMember(Value = "E1002")]
    UnexpectedEscape = 1002,

    [EnumMember(Value = "E1003")]
    UnterminatedStringLiteral = 1003,

    [EnumMember(Value = "E1004")]
    UnterminatedCharacterLiteral = 1004,

    [EnumMember(Value = "E1005")]
    UnterminatedBlockComment = 1005,

    // -- Semantic (2000-2999) ---
    [EnumMember(Value = "E2000")]
    UnresolvedSymbol = 2000,

    [EnumMember(Value = "E2001")]
    InvalidSymbol = 2001,

    [EnumMember(Value = "E2002")]
    ExpectedTypeSpecifier = 2002,

    [EnumMember(Value = "E2003")]
    AmbiguousSymbol = 2003,

    [EnumMember(Value = "E2004")]
    SymbolInaccessible = 2004,

    [EnumMember(Value = "E2005")]
    NoConversion = 2005,

    [EnumMember(Value = "E2006")]
    ConversionIsExplicit = 2006,

    [EnumMember(Value = "E2007")]
    MultipleFileScopedNamespaces = 2007,

    [EnumMember(Value = "E2008")]
    FileScopedAndNormalNamespace = 2008,

    [EnumMember(Value = "E2009")]
    FileScopedNamespaceNotBeforeAllMembers = 2009,

    [EnumMember(Value = "E2010")]
    BadModifiersOnNamespace = 2010,

    [EnumMember(Value = "E2011")]
    LiteralValueTooBig = 2011,

    [EnumMember(Value = "E2012")]
    UnaryOperatorUndefined = 2012,

    [EnumMember(Value = "E2013")]
    CannotAssignExpression = 2013,

    [EnumMember(Value = "E2014")]
    NoCompoundAssignmentOperator = 2014,

    [EnumMember(Value = "E2015")]
    BinaryOperatorUndefined = 2015,

    [EnumMember(Value = "E2016")]
    NoOverloadMatchingArgCount = 2016,

    [EnumMember(Value = "E2017")]
    NoOverloadForArgTypes = 2017,

    [EnumMember(Value = "E2018")]
    AmbiguousOverloadDefined = 2018,

    [EnumMember(Value = "E2019")]
    NoCallOperatorDefined = 2019,

    [EnumMember(Value = "E2020")]
    NoEntryPointDefined = 2020,

    [EnumMember(Value = "E2021")]
    AmbiguousEntryPoint = 2021,

    [EnumMember(Value = "E2022")]
    InvalidEntryPointReturnType = 2022,

    [EnumMember(Value = "E2023")]
    InvalidEntryPointParameters = 2023,

    [EnumMember(Value = "E2024")]
    InvalidUseOfLabel = 2024,

    [EnumMember(Value = "E2025")]
    InvalidUseOfBreak = 2025,

    [EnumMember(Value = "E2026")]
    InvalidUseOfContinue = 2026,

    [EnumMember(Value = "E2027")]
    CannotTakeAddress = 2027,

    [EnumMember(Value = "E2028")]
    CannotDereference = 2028,

    [EnumMember(Value = "E2029")]
    CannotDereferenceVariableLength = 2029,

    [EnumMember(Value = "E2030")]
    NoIndexOperation = 2030,

    [EnumMember(Value = "E2031")]
    AllPathsMustReturnValue = 2031,

    [EnumMember(Value = "E2032")]
    StatementMustReturnValue = 2032,

    [EnumMember(Value = "E2033")]
    CannotReturnValue = 2033,

    [EnumMember(Value = "E2034")]
    UnreachableCode = 2034,

    [EnumMember(Value = "E2035")]
    CannotReassign = 2035,

    [EnumMember(Value = "E2036")]
    ValueUninitialized = 2036,

    [EnumMember(Value = "E2037")]
    NotInitializedOnAllPaths = 2037,

    [EnumMember(Value = "E2038")]
    IncompatibleCollectionSizes = 2038,

    [EnumMember(Value = "E2039")]
    CannotInferType = 2039,

    // -- Code gen (3000-3999) ---
    [EnumMember(Value = "E3000")]
    ExternalDiagnostic = 3000,
}
