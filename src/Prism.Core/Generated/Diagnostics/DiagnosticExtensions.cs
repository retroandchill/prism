// Generated file, do not edit
namespace Prism.Core.Diagnostics;

public static class DiagnosticExtensions
{
    extension(DiagnosticInfo)
    {
        internal static DiagnosticInfo UnexpectedToken(string token)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedToken, token);
        }

        internal static DiagnosticInfo UnexpectedToken(DiagnosticSeverity severity, string token)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedToken, severity, token);
        }

        internal static DiagnosticInfo UnexpectedEOF()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedEOF);
        }

        internal static DiagnosticInfo UnexpectedEOF(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedEOF, severity);
        }

        internal static DiagnosticInfo UnexpectedEscape(string sequence)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedEscape, sequence);
        }

        internal static DiagnosticInfo UnexpectedEscape(
            DiagnosticSeverity severity,
            string sequence
        )
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnexpectedEscape, severity, sequence);
        }

        internal static DiagnosticInfo UnterminatedStringLiteral()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedStringLiteral);
        }

        internal static DiagnosticInfo UnterminatedStringLiteral(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedStringLiteral, severity);
        }

        internal static DiagnosticInfo UnterminatedCharacterLiteral()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedCharacterLiteral);
        }

        internal static DiagnosticInfo UnterminatedCharacterLiteral(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedCharacterLiteral, severity);
        }

        internal static DiagnosticInfo UnterminatedBlockComment()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedBlockComment);
        }

        internal static DiagnosticInfo UnterminatedBlockComment(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnterminatedBlockComment, severity);
        }

        internal static DiagnosticInfo UnresolvedSymbol(string symbol)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnresolvedSymbol, symbol);
        }

        internal static DiagnosticInfo UnresolvedSymbol(DiagnosticSeverity severity, string symbol)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnresolvedSymbol, severity, symbol);
        }

        internal static DiagnosticInfo InvalidSymbol(string symbol, string expected)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidSymbol, symbol, expected);
        }

        internal static DiagnosticInfo InvalidSymbol(
            DiagnosticSeverity severity,
            string symbol,
            string expected
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.InvalidSymbol,
                severity,
                symbol,
                expected
            );
        }

        internal static DiagnosticInfo ExpectedTypeSpecifier()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.ExpectedTypeSpecifier);
        }

        internal static DiagnosticInfo ExpectedTypeSpecifier(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.ExpectedTypeSpecifier, severity);
        }

        internal static DiagnosticInfo AmbiguousSymbol(string symbol)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.AmbiguousSymbol, symbol);
        }

        internal static DiagnosticInfo AmbiguousSymbol(DiagnosticSeverity severity, string symbol)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.AmbiguousSymbol, severity, symbol);
        }

        internal static DiagnosticInfo SymbolInaccessible(string symbol)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.SymbolInaccessible, symbol);
        }

        internal static DiagnosticInfo SymbolInaccessible(
            DiagnosticSeverity severity,
            string symbol
        )
        {
            return new DiagnosticInfo(DiagnosticDescriptors.SymbolInaccessible, severity, symbol);
        }

        internal static DiagnosticInfo NoConversion(string fromType, string toType)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoConversion, fromType, toType);
        }

        internal static DiagnosticInfo NoConversion(
            DiagnosticSeverity severity,
            string fromType,
            string toType
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.NoConversion,
                severity,
                fromType,
                toType
            );
        }

        internal static DiagnosticInfo ConversionIsExplicit(string fromType, string toType)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.ConversionIsExplicit, fromType, toType);
        }

        internal static DiagnosticInfo ConversionIsExplicit(
            DiagnosticSeverity severity,
            string fromType,
            string toType
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.ConversionIsExplicit,
                severity,
                fromType,
                toType
            );
        }

        internal static DiagnosticInfo MultipleFileScopedNamespaces()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.MultipleFileScopedNamespaces);
        }

        internal static DiagnosticInfo MultipleFileScopedNamespaces(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.MultipleFileScopedNamespaces, severity);
        }

        internal static DiagnosticInfo FileScopedAndNormalNamespace()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.FileScopedAndNormalNamespace);
        }

        internal static DiagnosticInfo FileScopedAndNormalNamespace(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.FileScopedAndNormalNamespace, severity);
        }

        internal static DiagnosticInfo FileScopedNamespaceNotBeforeAllMembers()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers);
        }

        internal static DiagnosticInfo FileScopedNamespaceNotBeforeAllMembers(
            DiagnosticSeverity severity
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers,
                severity
            );
        }

        internal static DiagnosticInfo BadModifiersOnNamespace()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.BadModifiersOnNamespace);
        }

        internal static DiagnosticInfo BadModifiersOnNamespace(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.BadModifiersOnNamespace, severity);
        }

        internal static DiagnosticInfo LiteralValueTooBig()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.LiteralValueTooBig);
        }

        internal static DiagnosticInfo LiteralValueTooBig(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.LiteralValueTooBig, severity);
        }

        internal static DiagnosticInfo UnaryOperatorUndefined(string type)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnaryOperatorUndefined, type);
        }

        internal static DiagnosticInfo UnaryOperatorUndefined(
            DiagnosticSeverity severity,
            string type
        )
        {
            return new DiagnosticInfo(DiagnosticDescriptors.UnaryOperatorUndefined, severity, type);
        }

        internal static DiagnosticInfo CannotAssignExpression()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotAssignExpression);
        }

        internal static DiagnosticInfo CannotAssignExpression(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotAssignExpression, severity);
        }

        internal static DiagnosticInfo NoCompoundAssignmentOperator(string type)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoCompoundAssignmentOperator, type);
        }

        internal static DiagnosticInfo NoCompoundAssignmentOperator(
            DiagnosticSeverity severity,
            string type
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.NoCompoundAssignmentOperator,
                severity,
                type
            );
        }

        internal static DiagnosticInfo BinaryOperatorUndefined(string type1, string type2)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.BinaryOperatorUndefined, type1, type2);
        }

        internal static DiagnosticInfo BinaryOperatorUndefined(
            DiagnosticSeverity severity,
            string type1,
            string type2
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.BinaryOperatorUndefined,
                severity,
                type1,
                type2
            );
        }

        internal static DiagnosticInfo NoOverloadMatchingArgCount(int argCount)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoOverloadMatchingArgCount, argCount);
        }

        internal static DiagnosticInfo NoOverloadMatchingArgCount(
            DiagnosticSeverity severity,
            int argCount
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.NoOverloadMatchingArgCount,
                severity,
                argCount
            );
        }

        internal static DiagnosticInfo NoOverloadForArgTypes(string argTypes)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoOverloadForArgTypes, argTypes);
        }

        internal static DiagnosticInfo NoOverloadForArgTypes(
            DiagnosticSeverity severity,
            string argTypes
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.NoOverloadForArgTypes,
                severity,
                argTypes
            );
        }

        internal static DiagnosticInfo NoCallOperatorDefined(string type)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoCallOperatorDefined, type);
        }

        internal static DiagnosticInfo NoCallOperatorDefined(
            DiagnosticSeverity severity,
            string type
        )
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoCallOperatorDefined, severity, type);
        }

        internal static DiagnosticInfo NoEntryPointDefined()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoEntryPointDefined);
        }

        internal static DiagnosticInfo NoEntryPointDefined(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.NoEntryPointDefined, severity);
        }

        internal static DiagnosticInfo AmbiguousEntryPoint(string names)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.AmbiguousEntryPoint, names);
        }

        internal static DiagnosticInfo AmbiguousEntryPoint(
            DiagnosticSeverity severity,
            string names
        )
        {
            return new DiagnosticInfo(DiagnosticDescriptors.AmbiguousEntryPoint, severity, names);
        }

        internal static DiagnosticInfo InvalidEntryPointReturnType(string type, string name)
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                type,
                name
            );
        }

        internal static DiagnosticInfo InvalidEntryPointReturnType(
            DiagnosticSeverity severity,
            string type,
            string name
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                severity,
                type,
                name
            );
        }

        internal static DiagnosticInfo InvalidEntryPointParameters(string types, string name)
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                types,
                name
            );
        }

        internal static DiagnosticInfo InvalidEntryPointParameters(
            DiagnosticSeverity severity,
            string types,
            string name
        )
        {
            return new DiagnosticInfo(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                severity,
                types,
                name
            );
        }

        internal static DiagnosticInfo InvalidUseOfLabel()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfLabel);
        }

        internal static DiagnosticInfo InvalidUseOfLabel(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfLabel, severity);
        }

        internal static DiagnosticInfo InvalidUseOfBreak()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfBreak);
        }

        internal static DiagnosticInfo InvalidUseOfBreak(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfBreak, severity);
        }

        internal static DiagnosticInfo InvalidUseOfContinue()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfContinue);
        }

        internal static DiagnosticInfo InvalidUseOfContinue(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.InvalidUseOfContinue, severity);
        }

        internal static DiagnosticInfo CannotTakeAddress()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotTakeAddress);
        }

        internal static DiagnosticInfo CannotTakeAddress(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotTakeAddress, severity);
        }

        internal static DiagnosticInfo CannotDereference()
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotDereference);
        }

        internal static DiagnosticInfo CannotDereference(DiagnosticSeverity severity)
        {
            return new DiagnosticInfo(DiagnosticDescriptors.CannotDereference, severity);
        }
    }

    extension(Diagnostic)
    {
        public static Diagnostic UnexpectedToken(Location location, string token)
        {
            return new Diagnostic(DiagnosticDescriptors.UnexpectedToken, location, token);
        }

        public static Diagnostic UnexpectedToken(
            DiagnosticSeverity severity,
            Location location,
            string token
        )
        {
            return new Diagnostic(DiagnosticDescriptors.UnexpectedToken, severity, location, token);
        }

        public static Diagnostic UnexpectedToken(
            Location location,
            IEnumerable<Location> additionalLocations,
            string token
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedToken,
                location,
                additionalLocations,
                token
            );
        }

        public static Diagnostic UnexpectedToken(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string token
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedToken,
                severity,
                location,
                additionalLocations,
                token
            );
        }

        public static Diagnostic UnexpectedEOF(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.UnexpectedEOF, location);
        }

        public static Diagnostic UnexpectedEOF(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.UnexpectedEOF, severity, location);
        }

        public static Diagnostic UnexpectedEOF(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedEOF,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnexpectedEOF(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedEOF,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnexpectedEscape(Location location, string sequence)
        {
            return new Diagnostic(DiagnosticDescriptors.UnexpectedEscape, location, sequence);
        }

        public static Diagnostic UnexpectedEscape(
            DiagnosticSeverity severity,
            Location location,
            string sequence
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedEscape,
                severity,
                location,
                sequence
            );
        }

        public static Diagnostic UnexpectedEscape(
            Location location,
            IEnumerable<Location> additionalLocations,
            string sequence
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedEscape,
                location,
                additionalLocations,
                sequence
            );
        }

        public static Diagnostic UnexpectedEscape(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string sequence
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnexpectedEscape,
                severity,
                location,
                additionalLocations,
                sequence
            );
        }

        public static Diagnostic UnterminatedStringLiteral(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.UnterminatedStringLiteral, location);
        }

        public static Diagnostic UnterminatedStringLiteral(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedStringLiteral,
                severity,
                location
            );
        }

        public static Diagnostic UnterminatedStringLiteral(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedStringLiteral,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnterminatedStringLiteral(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedStringLiteral,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnterminatedCharacterLiteral(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.UnterminatedCharacterLiteral, location);
        }

        public static Diagnostic UnterminatedCharacterLiteral(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedCharacterLiteral,
                severity,
                location
            );
        }

        public static Diagnostic UnterminatedCharacterLiteral(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedCharacterLiteral,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnterminatedCharacterLiteral(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedCharacterLiteral,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnterminatedBlockComment(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.UnterminatedBlockComment, location);
        }

        public static Diagnostic UnterminatedBlockComment(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedBlockComment,
                severity,
                location
            );
        }

        public static Diagnostic UnterminatedBlockComment(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedBlockComment,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnterminatedBlockComment(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnterminatedBlockComment,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnresolvedSymbol(Location location, string symbol)
        {
            return new Diagnostic(DiagnosticDescriptors.UnresolvedSymbol, location, symbol);
        }

        public static Diagnostic UnresolvedSymbol(
            DiagnosticSeverity severity,
            Location location,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnresolvedSymbol,
                severity,
                location,
                symbol
            );
        }

        public static Diagnostic UnresolvedSymbol(
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnresolvedSymbol,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic UnresolvedSymbol(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnresolvedSymbol,
                severity,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic InvalidSymbol(Location location, string symbol, string expected)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidSymbol, location, symbol, expected);
        }

        public static Diagnostic InvalidSymbol(
            DiagnosticSeverity severity,
            Location location,
            string symbol,
            string expected
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidSymbol,
                severity,
                location,
                symbol,
                expected
            );
        }

        public static Diagnostic InvalidSymbol(
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol,
            string expected
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidSymbol,
                location,
                additionalLocations,
                symbol,
                expected
            );
        }

        public static Diagnostic InvalidSymbol(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol,
            string expected
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidSymbol,
                severity,
                location,
                additionalLocations,
                symbol,
                expected
            );
        }

        public static Diagnostic ExpectedTypeSpecifier(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.ExpectedTypeSpecifier, location);
        }

        public static Diagnostic ExpectedTypeSpecifier(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(DiagnosticDescriptors.ExpectedTypeSpecifier, severity, location);
        }

        public static Diagnostic ExpectedTypeSpecifier(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ExpectedTypeSpecifier,
                location,
                additionalLocations
            );
        }

        public static Diagnostic ExpectedTypeSpecifier(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ExpectedTypeSpecifier,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic AmbiguousSymbol(Location location, string symbol)
        {
            return new Diagnostic(DiagnosticDescriptors.AmbiguousSymbol, location, symbol);
        }

        public static Diagnostic AmbiguousSymbol(
            DiagnosticSeverity severity,
            Location location,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousSymbol,
                severity,
                location,
                symbol
            );
        }

        public static Diagnostic AmbiguousSymbol(
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousSymbol,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic AmbiguousSymbol(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousSymbol,
                severity,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic SymbolInaccessible(Location location, string symbol)
        {
            return new Diagnostic(DiagnosticDescriptors.SymbolInaccessible, location, symbol);
        }

        public static Diagnostic SymbolInaccessible(
            DiagnosticSeverity severity,
            Location location,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.SymbolInaccessible,
                severity,
                location,
                symbol
            );
        }

        public static Diagnostic SymbolInaccessible(
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.SymbolInaccessible,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic SymbolInaccessible(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string symbol
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.SymbolInaccessible,
                severity,
                location,
                additionalLocations,
                symbol
            );
        }

        public static Diagnostic NoConversion(Location location, string fromType, string toType)
        {
            return new Diagnostic(DiagnosticDescriptors.NoConversion, location, fromType, toType);
        }

        public static Diagnostic NoConversion(
            DiagnosticSeverity severity,
            Location location,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoConversion,
                severity,
                location,
                fromType,
                toType
            );
        }

        public static Diagnostic NoConversion(
            Location location,
            IEnumerable<Location> additionalLocations,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoConversion,
                location,
                additionalLocations,
                fromType,
                toType
            );
        }

        public static Diagnostic NoConversion(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoConversion,
                severity,
                location,
                additionalLocations,
                fromType,
                toType
            );
        }

        public static Diagnostic ConversionIsExplicit(
            Location location,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ConversionIsExplicit,
                location,
                fromType,
                toType
            );
        }

        public static Diagnostic ConversionIsExplicit(
            DiagnosticSeverity severity,
            Location location,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ConversionIsExplicit,
                severity,
                location,
                fromType,
                toType
            );
        }

        public static Diagnostic ConversionIsExplicit(
            Location location,
            IEnumerable<Location> additionalLocations,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ConversionIsExplicit,
                location,
                additionalLocations,
                fromType,
                toType
            );
        }

        public static Diagnostic ConversionIsExplicit(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string fromType,
            string toType
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.ConversionIsExplicit,
                severity,
                location,
                additionalLocations,
                fromType,
                toType
            );
        }

        public static Diagnostic MultipleFileScopedNamespaces(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.MultipleFileScopedNamespaces, location);
        }

        public static Diagnostic MultipleFileScopedNamespaces(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.MultipleFileScopedNamespaces,
                severity,
                location
            );
        }

        public static Diagnostic MultipleFileScopedNamespaces(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.MultipleFileScopedNamespaces,
                location,
                additionalLocations
            );
        }

        public static Diagnostic MultipleFileScopedNamespaces(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.MultipleFileScopedNamespaces,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic FileScopedAndNormalNamespace(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.FileScopedAndNormalNamespace, location);
        }

        public static Diagnostic FileScopedAndNormalNamespace(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedAndNormalNamespace,
                severity,
                location
            );
        }

        public static Diagnostic FileScopedAndNormalNamespace(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedAndNormalNamespace,
                location,
                additionalLocations
            );
        }

        public static Diagnostic FileScopedAndNormalNamespace(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedAndNormalNamespace,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic FileScopedNamespaceNotBeforeAllMembers(Location location)
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers,
                location
            );
        }

        public static Diagnostic FileScopedNamespaceNotBeforeAllMembers(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers,
                severity,
                location
            );
        }

        public static Diagnostic FileScopedNamespaceNotBeforeAllMembers(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers,
                location,
                additionalLocations
            );
        }

        public static Diagnostic FileScopedNamespaceNotBeforeAllMembers(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.FileScopedNamespaceNotBeforeAllMembers,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic BadModifiersOnNamespace(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.BadModifiersOnNamespace, location);
        }

        public static Diagnostic BadModifiersOnNamespace(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BadModifiersOnNamespace,
                severity,
                location
            );
        }

        public static Diagnostic BadModifiersOnNamespace(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BadModifiersOnNamespace,
                location,
                additionalLocations
            );
        }

        public static Diagnostic BadModifiersOnNamespace(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BadModifiersOnNamespace,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic LiteralValueTooBig(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.LiteralValueTooBig, location);
        }

        public static Diagnostic LiteralValueTooBig(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.LiteralValueTooBig, severity, location);
        }

        public static Diagnostic LiteralValueTooBig(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.LiteralValueTooBig,
                location,
                additionalLocations
            );
        }

        public static Diagnostic LiteralValueTooBig(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.LiteralValueTooBig,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic UnaryOperatorUndefined(Location location, string type)
        {
            return new Diagnostic(DiagnosticDescriptors.UnaryOperatorUndefined, location, type);
        }

        public static Diagnostic UnaryOperatorUndefined(
            DiagnosticSeverity severity,
            Location location,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnaryOperatorUndefined,
                severity,
                location,
                type
            );
        }

        public static Diagnostic UnaryOperatorUndefined(
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnaryOperatorUndefined,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic UnaryOperatorUndefined(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.UnaryOperatorUndefined,
                severity,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic CannotAssignExpression(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.CannotAssignExpression, location);
        }

        public static Diagnostic CannotAssignExpression(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(DiagnosticDescriptors.CannotAssignExpression, severity, location);
        }

        public static Diagnostic CannotAssignExpression(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotAssignExpression,
                location,
                additionalLocations
            );
        }

        public static Diagnostic CannotAssignExpression(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotAssignExpression,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic NoCompoundAssignmentOperator(Location location, string type)
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCompoundAssignmentOperator,
                location,
                type
            );
        }

        public static Diagnostic NoCompoundAssignmentOperator(
            DiagnosticSeverity severity,
            Location location,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCompoundAssignmentOperator,
                severity,
                location,
                type
            );
        }

        public static Diagnostic NoCompoundAssignmentOperator(
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCompoundAssignmentOperator,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic NoCompoundAssignmentOperator(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCompoundAssignmentOperator,
                severity,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic BinaryOperatorUndefined(
            Location location,
            string type1,
            string type2
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BinaryOperatorUndefined,
                location,
                type1,
                type2
            );
        }

        public static Diagnostic BinaryOperatorUndefined(
            DiagnosticSeverity severity,
            Location location,
            string type1,
            string type2
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BinaryOperatorUndefined,
                severity,
                location,
                type1,
                type2
            );
        }

        public static Diagnostic BinaryOperatorUndefined(
            Location location,
            IEnumerable<Location> additionalLocations,
            string type1,
            string type2
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BinaryOperatorUndefined,
                location,
                additionalLocations,
                type1,
                type2
            );
        }

        public static Diagnostic BinaryOperatorUndefined(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string type1,
            string type2
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.BinaryOperatorUndefined,
                severity,
                location,
                additionalLocations,
                type1,
                type2
            );
        }

        public static Diagnostic NoOverloadMatchingArgCount(Location location, int argCount)
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadMatchingArgCount,
                location,
                argCount
            );
        }

        public static Diagnostic NoOverloadMatchingArgCount(
            DiagnosticSeverity severity,
            Location location,
            int argCount
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadMatchingArgCount,
                severity,
                location,
                argCount
            );
        }

        public static Diagnostic NoOverloadMatchingArgCount(
            Location location,
            IEnumerable<Location> additionalLocations,
            int argCount
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadMatchingArgCount,
                location,
                additionalLocations,
                argCount
            );
        }

        public static Diagnostic NoOverloadMatchingArgCount(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            int argCount
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadMatchingArgCount,
                severity,
                location,
                additionalLocations,
                argCount
            );
        }

        public static Diagnostic NoOverloadForArgTypes(Location location, string argTypes)
        {
            return new Diagnostic(DiagnosticDescriptors.NoOverloadForArgTypes, location, argTypes);
        }

        public static Diagnostic NoOverloadForArgTypes(
            DiagnosticSeverity severity,
            Location location,
            string argTypes
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadForArgTypes,
                severity,
                location,
                argTypes
            );
        }

        public static Diagnostic NoOverloadForArgTypes(
            Location location,
            IEnumerable<Location> additionalLocations,
            string argTypes
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadForArgTypes,
                location,
                additionalLocations,
                argTypes
            );
        }

        public static Diagnostic NoOverloadForArgTypes(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string argTypes
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoOverloadForArgTypes,
                severity,
                location,
                additionalLocations,
                argTypes
            );
        }

        public static Diagnostic NoCallOperatorDefined(Location location, string type)
        {
            return new Diagnostic(DiagnosticDescriptors.NoCallOperatorDefined, location, type);
        }

        public static Diagnostic NoCallOperatorDefined(
            DiagnosticSeverity severity,
            Location location,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCallOperatorDefined,
                severity,
                location,
                type
            );
        }

        public static Diagnostic NoCallOperatorDefined(
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCallOperatorDefined,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic NoCallOperatorDefined(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string type
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoCallOperatorDefined,
                severity,
                location,
                additionalLocations,
                type
            );
        }

        public static Diagnostic NoEntryPointDefined(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.NoEntryPointDefined, location);
        }

        public static Diagnostic NoEntryPointDefined(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.NoEntryPointDefined, severity, location);
        }

        public static Diagnostic NoEntryPointDefined(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoEntryPointDefined,
                location,
                additionalLocations
            );
        }

        public static Diagnostic NoEntryPointDefined(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.NoEntryPointDefined,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic AmbiguousEntryPoint(Location location, string names)
        {
            return new Diagnostic(DiagnosticDescriptors.AmbiguousEntryPoint, location, names);
        }

        public static Diagnostic AmbiguousEntryPoint(
            DiagnosticSeverity severity,
            Location location,
            string names
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousEntryPoint,
                severity,
                location,
                names
            );
        }

        public static Diagnostic AmbiguousEntryPoint(
            Location location,
            IEnumerable<Location> additionalLocations,
            string names
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousEntryPoint,
                location,
                additionalLocations,
                names
            );
        }

        public static Diagnostic AmbiguousEntryPoint(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string names
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.AmbiguousEntryPoint,
                severity,
                location,
                additionalLocations,
                names
            );
        }

        public static Diagnostic InvalidEntryPointReturnType(
            Location location,
            string type,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                location,
                type,
                name
            );
        }

        public static Diagnostic InvalidEntryPointReturnType(
            DiagnosticSeverity severity,
            Location location,
            string type,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                severity,
                location,
                type,
                name
            );
        }

        public static Diagnostic InvalidEntryPointReturnType(
            Location location,
            IEnumerable<Location> additionalLocations,
            string type,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                location,
                additionalLocations,
                type,
                name
            );
        }

        public static Diagnostic InvalidEntryPointReturnType(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string type,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointReturnType,
                severity,
                location,
                additionalLocations,
                type,
                name
            );
        }

        public static Diagnostic InvalidEntryPointParameters(
            Location location,
            string types,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                location,
                types,
                name
            );
        }

        public static Diagnostic InvalidEntryPointParameters(
            DiagnosticSeverity severity,
            Location location,
            string types,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                severity,
                location,
                types,
                name
            );
        }

        public static Diagnostic InvalidEntryPointParameters(
            Location location,
            IEnumerable<Location> additionalLocations,
            string types,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                location,
                additionalLocations,
                types,
                name
            );
        }

        public static Diagnostic InvalidEntryPointParameters(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations,
            string types,
            string name
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidEntryPointParameters,
                severity,
                location,
                additionalLocations,
                types,
                name
            );
        }

        public static Diagnostic InvalidUseOfLabel(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfLabel, location);
        }

        public static Diagnostic InvalidUseOfLabel(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfLabel, severity, location);
        }

        public static Diagnostic InvalidUseOfLabel(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfLabel,
                location,
                additionalLocations
            );
        }

        public static Diagnostic InvalidUseOfLabel(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfLabel,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic InvalidUseOfBreak(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfBreak, location);
        }

        public static Diagnostic InvalidUseOfBreak(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfBreak, severity, location);
        }

        public static Diagnostic InvalidUseOfBreak(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfBreak,
                location,
                additionalLocations
            );
        }

        public static Diagnostic InvalidUseOfBreak(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfBreak,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic InvalidUseOfContinue(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfContinue, location);
        }

        public static Diagnostic InvalidUseOfContinue(
            DiagnosticSeverity severity,
            Location location
        )
        {
            return new Diagnostic(DiagnosticDescriptors.InvalidUseOfContinue, severity, location);
        }

        public static Diagnostic InvalidUseOfContinue(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfContinue,
                location,
                additionalLocations
            );
        }

        public static Diagnostic InvalidUseOfContinue(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.InvalidUseOfContinue,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic CannotTakeAddress(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.CannotTakeAddress, location);
        }

        public static Diagnostic CannotTakeAddress(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.CannotTakeAddress, severity, location);
        }

        public static Diagnostic CannotTakeAddress(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotTakeAddress,
                location,
                additionalLocations
            );
        }

        public static Diagnostic CannotTakeAddress(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotTakeAddress,
                severity,
                location,
                additionalLocations
            );
        }

        public static Diagnostic CannotDereference(Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.CannotDereference, location);
        }

        public static Diagnostic CannotDereference(DiagnosticSeverity severity, Location location)
        {
            return new Diagnostic(DiagnosticDescriptors.CannotDereference, severity, location);
        }

        public static Diagnostic CannotDereference(
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotDereference,
                location,
                additionalLocations
            );
        }

        public static Diagnostic CannotDereference(
            DiagnosticSeverity severity,
            Location location,
            IEnumerable<Location> additionalLocations
        )
        {
            return new Diagnostic(
                DiagnosticDescriptors.CannotDereference,
                severity,
                location,
                additionalLocations
            );
        }
    }
}
