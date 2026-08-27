// @file DiagnosticGenerator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prism.Core.Diagnostics;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Prism.SourceGenerator.Diagnostics;

[Generator]
public sealed class DiagnosticGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor DiagnosticClassMustBePartial = new(
        "PRSM1001",
        "Owning diagnostic class must be partial",
        "Class '{0}' must be partial",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor DiagnosticMethodMustBePartial = new(
        "PRSM1002",
        "Diagnostic method must be partial",
        "Method '{0}' must be partial",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor MethodMustReturnDiagnostic = new(
        "PRSM1003",
        "Method must return Diagnostic",
        "Method '{0}' must return Diagnostic",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor MustHaveAtLeastOneParameter = new(
        "PRSM1004",
        "Diagnostic method must have at least once param",
        "Method '{0}' must have at least one parameter",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor ParamMustBeSourceLocation = new(
        "PRSM1005",
        "Parameter must be of type Location",
        "Parameter '{0}' must be type Location",
        "Prism",
        DiagnosticSeverity.Error,
        true
    );

    private DiagnosticEmitter _emitter = null!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _emitter = new DiagnosticEmitter();
        var fullName = typeof(DiagnosticAttribute).FullName!;
        var methods = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullName,
            static (node, _) => node is MethodDeclarationSyntax,
            static (node, _) => (IMethodSymbol)node.TargetSymbol
        );

        context.RegisterSourceOutput(methods, Emit);
    }

    private void Emit(SourceProductionContext context, IMethodSymbol method)
    {
        var containingType = method.ContainingType;
        var info = method.GetDiagnosticInfo();

        var classType = containingType switch
        {
            { IsValueType: true, IsRecord: true } => "record struct",
            { IsValueType: true } => "struct",
            { IsRecord: true } => "record",
            _ => "class",
        };

        var accessType = method.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private",
        };

        var isValid = true;

        if (!ValidatePartial(containingType))
        {
            isValid = false;
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticClassMustBePartial, method.Locations[0], method.Name)
            );
        }

        if (!ValidatePartial(method))
        {
            isValid = false;
            context.ReportDiagnostic(
                Diagnostic.Create(DiagnosticMethodMustBePartial, method.Locations[0], method.Name)
            );
        }

        var returnsDiagnostic = true;
        var returnType = method.ReturnType;
        switch (returnType.ToDisplayString())
        {
            case DiagnosticSymbols.Diagnostic:
                if (method.Parameters.Length < 1)
                {
                    isValid = false;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            MustHaveAtLeastOneParameter,
                            method.Locations[0],
                            method.Name
                        )
                    );
                }
                else if (method.Parameters[0].Type.ToDisplayString() != DiagnosticSymbols.Location)
                {
                    isValid = false;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            ParamMustBeSourceLocation,
                            method.Parameters[0].Locations[0],
                            method.Parameters[0].Name
                        )
                    );
                }
                break;
            case DiagnosticSymbols.DiagnosticInfo:
                returnsDiagnostic = false;
                break;
            default:
                isValid = false;
                context.ReportDiagnostic(
                    Diagnostic.Create(MethodMustReturnDiagnostic, method.Locations[0], method.Name)
                );
                break;
        }

        if (!isValid)
            return;

        var offset = returnsDiagnostic ? 1 : 0;
        var builder = ImmutableArray.CreateBuilder<FormatParam>(method.Parameters.Length - offset);
        for (var i = offset; i < method.Parameters.Length; i++)
        {
            builder.Add(
                new FormatParam
                {
                    Type = method.Parameters[i].Type.ToDisplayString(),
                    Name = method.Parameters[i].Name,
                }
            );
        }

        var payload = new GeneratedDiagnostic
        {
            Namespace = containingType.ContainingNamespace.ToDisplayString(),
            ClassType = classType,
            ClassName = containingType.Name,
            Access = accessType,
            Static = method.IsStatic ? " static" : "",
            MethodName = method.Name,
            IsExtension = method.IsExtensionMethod,
            LocationParam = returnsDiagnostic ? method.Parameters[0].Name : null,
            FormatParams = builder.DrainToImmutable(),
            ReturnsDiagnostic = returnsDiagnostic,
            Id = SymbolDisplay.FormatLiteral(info.Id, true),
            Title = SymbolDisplay.FormatLiteral(info.Title, true),
            Description = info.Description is not null
                ? SymbolDisplay.FormatLiteral(info.Description, true)
                : null,
            Severity = info.Severity,
            MessageFormat = SymbolDisplay.FormatLiteral(info.MessageFormat, true),
        };

        context.AddSource($"{containingType.Name}.{method.Name}.g.cs", _emitter.Emit(payload));
    }

    private static bool ValidatePartial(ISymbol symbol)
    {
        return symbol
            .DeclaringSyntaxReferences.Select(x => x.GetSyntax())
            .OfType<MemberDeclarationSyntax>()
            .Any(syntax => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }
}
