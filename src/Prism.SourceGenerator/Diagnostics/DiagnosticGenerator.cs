// @file DiagnosticGenerator.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prism.Core.Diagnostics;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using System.Text;

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
        true);
    
    private static readonly DiagnosticDescriptor DiagnosticMethodMustBePartial = new(
        "PRSM1002",
        "Diagnostic method must be partial",
        "Method '{0}' must be partial",
        "Prism",
        DiagnosticSeverity.Error,
        true);
    
    private static readonly DiagnosticDescriptor DiagnosticMethodMustHaveAtLeast2Params = new(
        "PRSM1003",
        "Diagnostic method must have 2 parameters",
        "Method '{0}' must have at least two parameters",
        "Prism",
        DiagnosticSeverity.Error,
        true);
    
    private static readonly DiagnosticDescriptor ParamMustImplementIDiagnosticSink = new(
        "PRSM1004",
        "Parameter must implement IDiagnostic sink",
        "Parameter '{0}' must be a type that is is or implements IDiagnosticSink",
        "Prism",
        DiagnosticSeverity.Error,
        true);
    
    private static readonly DiagnosticDescriptor ParamMustBeSourceRange = new(
        "PRSM1004",
        "Parameter must be of type SourceRange",
        "Parameter '{0}' must be type SourceRange",
        "Prism",
        DiagnosticSeverity.Error,
        true);
    
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
            _ => "class"
        };
        
        var accessType = method.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "private"
        };

        var isValid = true;

        if (!ValidatePartial(containingType))
        {
            isValid = false;
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticClassMustBePartial, method.Locations[0], method.Name));
        }

        if (!ValidatePartial(method))
        {
            isValid = false;
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticMethodMustBePartial, method.Locations[0], method.Name));
        }

        if (method.Parameters.Length < 2)
        {
            isValid = false;
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticMethodMustHaveAtLeast2Params, method.Locations[0], method.Name));
        }
        
        if (method.Parameters.Length > 0)
        {
            var sinkType = method.Parameters[0].Type;
            if (sinkType.ToDisplayString() != DiagnosticSymbols.IDiagnosticSink && !sinkType.AllInterfaces.Any(x => x.ToDisplayString() == DiagnosticSymbols.IDiagnosticSink))
            {
                isValid = false;
                context.ReportDiagnostic(Diagnostic.Create(ParamMustImplementIDiagnosticSink, method.Locations[0], method.Parameters[0].Name));
            }
        }
        
        if (method.Parameters.Length > 1)
        {
            var sourceRangeType = method.Parameters[1].Type;
            if (sourceRangeType.ToDisplayString() != DiagnosticSymbols.SourceRange)
            {
                isValid = false;
                context.ReportDiagnostic(Diagnostic.Create(ParamMustBeSourceRange, method.Locations[0], method.Parameters[1].Name));
            }
        }

        if (!isValid)
            return;
        
        var builder = ImmutableArray.CreateBuilder<FormatParam>(method.Parameters.Length - 2);
        for (var i = 2; i < method.Parameters.Length; i++)
        {
            builder.Add(new FormatParam
            {
                Type = method.Parameters[i].Type.ToDisplayString(),
                Name = method.Parameters[i].Name,
            });
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
            SinkType = method.Parameters[0].Type.ToDisplayString(),
            SinkParam = method.Parameters[0].Name,
            RangeParam = method.Parameters[1].Name,
            DiagnosticClass = method.Name.StartsWith("Report") ? method.Name[6..] : method.Name,
            FormatParams = builder.DrainToImmutable(),
            Id = SymbolDisplay.FormatLiteral(info.Id, true),
            Title = SymbolDisplay.FormatLiteral(info.Title, true),
            Description = info.Description is not null ? SymbolDisplay.FormatLiteral(info.Description, true) : null,
            Severity = info.Severity,
            MessageFormat = SymbolDisplay.FormatLiteral(info.MessageFormat, true),
        };

        context.AddSource($"{containingType.Name}.{method.Name}.g.cs", _emitter.Emit(payload));
    }

    private static bool ValidatePartial(ISymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<MemberDeclarationSyntax>().Any(syntax => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }
}