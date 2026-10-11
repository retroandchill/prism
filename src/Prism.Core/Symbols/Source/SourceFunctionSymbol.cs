using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Abi;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceFunctionSymbol : SourceFunctionSymbolBase
{
    internal SourceFunctionSymbol(
        string name,
        MemberContainerSymbol containingSymbol,
        FunctionDeclarationSyntax syntax
    )
        : base(name, containingSymbol, syntax)
    {
        Syntax = syntax;
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(Syntax, this);
    }

    public override string MetadataName
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ProduceMetadataName(Syntax.SyntaxTree), null);
            return field;
        }
    }

    public override FunctionKind Kind => FunctionKind.Ordinary;

    public FunctionDeclarationSyntax Syntax { get; }

    protected override DeclarationSyntax AbstractSyntax => Syntax;

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [Syntax.Identifier.Location],
                default
            );
            return field;
        }
    }

    protected override TypeSymbol ComputeReturnType(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);

        if (Syntax.ReturnType is null)
        {
            // Omitting the return type just results in void
            return compilation.GetSpecialType(SpecialType.Void);
        }

        var semanticModel = compilation.GetSemanticModel(Syntax.SyntaxTree);
        var binder = semanticModel.GetBinder(Syntax);
        return binder.ResolveType(Syntax.ReturnType.Type, context);
    }

    protected override TypeSymbol ComputeReceiverType(TypeSymbol containerType)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);

        if (Syntax.RefQualifier is null)
        {
            return compilation.CreateReferenceTypeSymbol(containerType);
        }

        return Syntax.RefQualifier.Value.Kind switch
        {
            SyntaxKind.MutableKeyword => compilation.CreateReferenceTypeSymbol(containerType, true),
            SyntaxKind.ValueKeyword => containerType,
            _ => throw new InvalidOperationException("Invalid ref qualifier"),
        };
    }

    protected override ImmutableArray<ParameterSymbol> ComputeParameters(BindingContext context)
    {
        var source = Syntax.Parameters.Parameters;
        return CreateParametersFromSource(source, context);
    }

    protected override void FunctionChecks(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        var intrinsicAttribute = compilation.GetWellKnownAttribute(WellKnownAttribute.Intrinsic);

        switch (this)
        {
            case { IsExtern: true, Syntax.Body: not null }:
                context.ReportDiagnostic(Diagnostic.ExternFunctionWithBody(Syntax.Location, Name));
                break;
            case { IsExtern: false, Syntax.Body: null }
                when GetAttributes().All(a => a.Attribute != intrinsicAttribute):
                context.ReportDiagnostic(Diagnostic.FunctionWithoutBody(Syntax.Location, Name));
                break;
        }
    }
}
