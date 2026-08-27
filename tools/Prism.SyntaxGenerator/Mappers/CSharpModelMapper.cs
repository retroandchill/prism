// @file CSharpModelMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Encodings.Web;
using System.Text.Json;
using Humanizer;
using Prism.SyntaxGenerator.Metadata;
using Prism.SyntaxGenerator.Models.CSharp;
using Prism.SyntaxGenerator.Models.Resolved;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.SyntaxGenerator.Mappers;

[Mapper(
    UseReferenceHandling = true,
    IncludedMembers = MemberVisibility.AllAccessible,
    IncludedConstructors = MemberVisibility.AllAccessible
)]
public static partial class CSharpModelMapper
{
    public static partial CSharpSyntaxModel ToCSharp(this SyntaxModel model);

    [MapProperty(
        nameof(SyntaxGroup.Name),
        nameof(CSharpGroup.CSharpName),
        Use = nameof(GetSingularName)
    )]
    [MapProperty(
        nameof(SyntaxGroup.Name),
        nameof(CSharpGroup.DisplayName),
        Use = nameof(GetHumanReadableName)
    )]
    private static partial CSharpGroup ToCSharp(
        this SyntaxGroup group,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxKind.Name),
        nameof(CSharpKind.CSharpName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    private static partial CSharpKind ToCSharp(
        this SyntaxKind kind,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxModule.Name),
        nameof(CSharpModule.CSharpName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    private static partial CSharpModule ToCSharp(
        this SyntaxModule module,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxNode.Name),
        nameof(CSharpNode.GreenClassName),
        Use = nameof(GetGreenCSharpName)
    )]
    [MapProperty(
        nameof(SyntaxNode.Name),
        nameof(CSharpNode.RedClassName),
        Use = nameof(GetRedCSharpName)
    )]
    private static partial CSharpNode ToCSharp(
        this SyntaxNode node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CSharpProduction ToCSharp(
        this SyntaxProduction node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxProperty.Name),
        nameof(CSharpProperty.PropertyName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    [MapProperty(
        nameof(SyntaxProperty.Name),
        nameof(CSharpProperty.ParameterName),
        Use = nameof(GetCSharpParameterName)
    )]
    private static partial CSharpProperty ToCSharp(
        this SyntaxProperty node,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CSharpTrivia ToCSharp(
        this SyntaxTrivia trivia,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    private static partial CSharpTypeReference ToCSharp(
        this SyntaxTypeReference trivia,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapDerivedType<SyntaxTrivia, CSharpTrivia>]
    [MapDerivedType<SyntaxToken, CSharpToken>]
    [MapDerivedType<SyntaxNode, CSharpNode>]
    private static partial ICSharpSyntaxVariant ToCSharp(
        this ISyntaxVariant variant,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(SyntaxDispatchGroup.Name),
        nameof(CSharpDispatchGroup.GreenClassName),
        Use = nameof(GetGreenCSharpName)
    )]
    [MapProperty(
        nameof(SyntaxDispatchGroup.Name),
        nameof(CSharpDispatchGroup.RedClassName),
        Use = nameof(GetRedCSharpName)
    )]
    private static partial CSharpDispatchGroup ToCSharp(
        this SyntaxDispatchGroup dispatchGroup,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticCategory.Name),
        nameof(CSharpDiagnosticCategory.CSharpName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    [MapProperty(
        nameof(DiagnosticCategory.Name),
        nameof(CSharpDiagnosticCategory.DisplayName),
        Use = nameof(GetHumanReadableName)
    )]
    private static partial CSharpDiagnosticCategory ToCSharp(
        this DiagnosticCategory category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CSharpDiagnostic.CSharpName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CSharpDiagnostic.Title),
        Use = nameof(GetHumanReadableName)
    )]
    [MapProperty(
        nameof(Diagnostic.Name),
        nameof(CSharpDiagnostic.SymbolName),
        Use = nameof(GetPascalizedName)
    )]
    [MapProperty(
        nameof(Diagnostic.Explanation),
        nameof(CSharpDiagnostic.Explanation),
        Use = nameof(GetEscapedCSharpText)
    )]
    private static partial CSharpDiagnostic ToCSharp(
        this Diagnostic category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticArgument.Name),
        nameof(CSharpDiagnosticArgument.CSharpName),
        Use = nameof(GetPublicCSharpMemberName)
    )]
    [MapProperty(
        nameof(DiagnosticArgument.Type),
        nameof(CSharpDiagnosticArgument.CSharpType),
        Use = nameof(GetCSharpFormatArgType)
    )]
    private static partial CSharpDiagnosticArgument ToCSharp(
        this DiagnosticArgument category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapProperty(
        nameof(DiagnosticMessageTextPart.Text),
        nameof(CSharpDiagnosticMessageTextPart.Text),
        Use = nameof(GetEscapedCSharpText)
    )]
    private static partial CSharpDiagnosticMessageTextPart ToCSharp(
        this DiagnosticMessageTextPart argument,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [MapDerivedType<DiagnosticMessageTextPart, CSharpDiagnosticMessageTextPart>]
    [MapDerivedType<DiagnosticMessageArgumentPart, CSharpDiagnosticMessageArgumentPart>]
    private static partial CSharpDiagnosticMessagePart ToCSharp(
        this DiagnosticMessagePart category,
        [ReferenceHandler] IReferenceHandler refHandler
    );

    [UserMapping(Default = false)]
    private static string GetPublicCSharpMemberName(string source)
    {
        return source.Pascalize();
    }

    [UserMapping(Default = false)]
    private static string GetCSharpParameterName(string source)
    {
        return source.Camelize();
    }

    [UserMapping(Default = false)]
    private static string GetSingularName(string name)
    {
        return name.Equals("Trivia", StringComparison.OrdinalIgnoreCase)
            ? "Trivia"
            : name.Singularize(false).Pascalize();
    }

    [UserMapping(Default = false)]
    private static string GetHumanReadableName(string name)
    {
        return name.Humanize();
    }

    [UserMapping(Default = false)]
    private static string GetGreenCSharpName(string name)
    {
        return $"Green{name.Pascalize()}";
    }

    [UserMapping(Default = false)]
    private static string GetRedCSharpName(string name)
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
    private static string GetCSharpFormatArgType(string name)
    {
        return name switch
        {
            "Int32" => "std::int32_t",
            "String" => "std::string",
            _ => name.Pascalize(),
        };
    }

    [UserMapping(Default = false)]
    private static string GetEscapedCSharpText(string name)
    {
        return JsonEncodedText.Encode(name, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).Value;
    }
}
