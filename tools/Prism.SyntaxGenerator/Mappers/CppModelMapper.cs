using System.Text.Encodings.Web;
using System.Text.Json;
using Humanizer;
using Prism.SyntaxGenerator.Metadata;
using Prism.SyntaxGenerator.Models.Cpp;
using Prism.SyntaxGenerator.Models.Resolved;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.SyntaxGenerator.Mappers;

[Mapper(
    UseReferenceHandling = true,
    IncludedMembers = MemberVisibility.AllAccessible,
    IncludedConstructors = MemberVisibility.AllAccessible
)]
public static partial class CppModelMapper
{
    public static partial CppSyntaxModel ToCpp(this SyntaxModel model);

    [MapProperty(nameof(SyntaxGroup.Name), nameof(CppGroup.CppName), Use = nameof(GetSingularName))]
    [MapProperty(
        nameof(SyntaxGroup.Name),
        nameof(CppGroup.DisplayName),
        Use = nameof(GetHumanReadableName)
    )]
    private static partial CppGroup ToCpp(
        this SyntaxGroup group,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxKind.Name),
        nameof(CppKind.CppName),
        Use = nameof(GetPublicCppMemberName)
    )]
    private static partial CppKind ToCpp(
        this SyntaxKind kind,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxModule.Name),
        nameof(CppModule.CppName),
        Use = nameof(GetPublicCppMemberName)
    )]
    private static partial CppModule ToCpp(
        this SyntaxModule module,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxNode.Name),
        nameof(CppNode.GreenClassName),
        Use = nameof(GetGreenCppName)
    )]
    [MapProperty(
        nameof(SyntaxNode.Name),
        nameof(CppNode.RedClassName),
        Use = nameof(GetRedCppName)
    )]
    private static partial CppNode ToCpp(
        this SyntaxNode node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CppProduction ToCpp(
        this SyntaxProduction node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxProperty.Name),
        nameof(CppProperty.GetterName),
        Use = nameof(GetPublicCppMemberName)
    )]
    [MapProperty(
        nameof(SyntaxProperty.Name),
        nameof(CppProperty.FieldName),
        Use = nameof(GetPrivateCppMemberName)
    )]
    [MapProperty(
        nameof(SyntaxProperty.Name),
        nameof(CppProperty.ParameterName),
        Use = nameof(GetPublicCppMemberName)
    )]
    private static partial CppProperty ToCpp(
        this SyntaxProperty node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CppTrivia ToCpp(
        this SyntaxTrivia trivia,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CppTypeReference ToCpp(
        this SyntaxTypeReference trivia,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapDerivedType<SyntaxTrivia, CppTrivia>]
    [MapDerivedType<SyntaxToken, CppToken>]
    [MapDerivedType<SyntaxNode, CppNode>]
    private static partial ICppSyntaxVariant ToCpp(
        this ISyntaxVariant variant,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxDispatchGroup.Name),
        nameof(CppDispatchGroup.GreenClassName),
        Use = nameof(GetGreenCppName)
    )]
    [MapProperty(
        nameof(SyntaxDispatchGroup.Name),
        nameof(CppDispatchGroup.RedClassName),
        Use = nameof(GetRedCppName)
    )]
    private static partial CppDispatchGroup ToCpp(
        this SyntaxDispatchGroup dispatchGroup,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticCategory.Name),
        nameof(CppDiagnosticCategory.CppName),
        Use = nameof(GetPublicCppMemberName)
    )]
    [MapProperty(
        nameof(DiagnosticCategory.Name),
        nameof(CppDiagnosticCategory.DisplayName),
        Use = nameof(GetHumanReadableName)
    )]
    private static partial CppDiagnosticCategory ToCpp(
        this DiagnosticCategory category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CppDiagnostic.CppName),
        Use = nameof(GetPublicCppMemberName)
    )]
    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CppDiagnostic.Title),
        Use = nameof(GetHumanReadableName)
    )]
    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CppDiagnostic.SymbolName),
        Use = nameof(GetPascalizedName)
    )]
    [MapProperty(
        nameof(Diagnostic.Explanation),
        nameof(CppDiagnostic.Explanation),
        Use = nameof(GetEscapedCppText)
    )]
    private static partial CppDiagnostic ToCpp(
        this Diagnostic category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticArgument.Name),
        nameof(CppDiagnosticArgument.CppName),
        Use = nameof(GetPublicCppMemberName)
    )]
    [MapProperty(
        nameof(DiagnosticArgument.Type),
        nameof(CppDiagnosticArgument.CppType),
        Use = nameof(GetCppFormatArgType)
    )]
    private static partial CppDiagnosticArgument ToCpp(
        this DiagnosticArgument category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticMessageTextPart.Text),
        nameof(CppDiagnosticMessageTextPart.Text),
        Use = nameof(GetEscapedCppText)
    )]
    private static partial CppDiagnosticMessageTextPart ToCpp(
        this DiagnosticMessageTextPart argument,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapDerivedType<DiagnosticMessageTextPart, CppDiagnosticMessageTextPart>]
    [MapDerivedType<DiagnosticMessageArgumentPart, CppDiagnosticMessageArgumentPart>]
    private static partial CppDiagnosticMessagePart ToCpp(
        this DiagnosticMessagePart category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [UserMapping(Default = false)]
    private static string GetPublicCppMemberName(string source)
    {
        return source.Underscore();
    }

    [UserMapping(Default = false)]
    private static string GetPrivateCppMemberName(string source)
    {
        return $"{source.Underscore()}_";
    }

    [UserMapping(Default = false)]
    private static string GetSingularName(string name)
    {
        return name.Equals("Trivia", StringComparison.OrdinalIgnoreCase)
            ? "trivia"
            : name.Singularize(false).Underscore();
    }

    [UserMapping(Default = false)]
    private static string GetHumanReadableName(string name)
    {
        return name.Humanize();
    }

    [UserMapping(Default = false)]
    private static string GetGreenCppName(string name)
    {
        return $"Green{name.Pascalize()}";
    }

    [UserMapping(Default = false)]
    private static string GetRedCppName(string name)
    {
        return name switch
        {
            "Trivia" => CommonNames.SyntaxTriviaClass,
            "Token" => CommonNames.SyntaxTokenClass,
            "Node" => CommonNames.SyntaxNodeClass,
            _ => $"{name.Pascalize()}Syntax",
        };
    }

    [UserMapping(Default = false)]
    private static string GetPascalizedName(string name)
    {
        return name.Pascalize();
    }

    [UserMapping(Default = false)]
    private static string GetCppFormatArgType(string name)
    {
        return name switch
        {
            "Int32" => "std::int32_t",
            "UInt32" => "std::uint32_t",
            "SizeT" => "std::size_t",
            "String" => "std::string",
            _ => name.Pascalize(),
        };
    }

    [UserMapping(Default = false)]
    private static string GetEscapedCppText(string name)
    {
        return JsonEncodedText.Encode(name, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).Value;
    }
}
