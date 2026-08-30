// @file SourceParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceParameterSymbol : ParameterSymbol
{
    private readonly ParameterSyntax _syntax;
    private SymbolCompletionState _completionState;

    internal SourceParameterSymbol(string name, Symbol? containingSymbol, ParameterSyntax syntax)
        : base(name, containingSymbol)
    {
        _syntax = syntax;

        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        compilation.CacheSymbol(_syntax, this);
    }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(ref field, [_syntax.Name.Location], default);
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Interlocked.CompareExchange(ref field, [new SyntaxReference(_syntax)], default);
            return field;
        }
    }

    public override TypeSymbol Type
    {
        get
        {
            if (field is not null)
                return field;

            var diagnostics = DiagnosticBag.Create();
            if (Interlocked.CompareExchange(ref field, ComputeType(diagnostics), null) is not null)
                return field;

            AddDeclarationDiagnostics(diagnostics);
            _completionState.MarkPartComplete(CompletionPart.Type);
            return field;
        }
    }

    private TypeSymbol ComputeType(DiagnosticBag diagnostics)
    {
        throw new NotImplementedException();
    }

    public override bool IsMutable => _syntax.MutableKeyword is not null;

    internal override bool NeedsCompletion => false;

    internal override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(this) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Type:
                    _ = Type;
                    break;
                default:
                    _completionState.MarkPartComplete(
                        CompletionPart.All & ~CompletionPart.ParameterAll
                    );
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    internal override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
