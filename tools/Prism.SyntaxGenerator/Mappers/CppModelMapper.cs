using Humanizer;
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
}
