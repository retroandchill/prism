using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.Diagnostics;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Symbols;

public closed class MemberContainerSymbol : Symbol
{
    private protected MemberContainerSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract ImmutableArray<Symbol> GetMembers();

    public abstract ImmutableArray<Symbol> GetMembers(string name);

    private protected static void ValidateMembers(
        string name,
        ImmutableArray<Symbol> members,
        BindingContext context
    )
    {
        if (members.Length == 1)
            return;

        Dictionary<OverloadSignature, List<Symbol>>? overloads = null;
        foreach (var member in members)
        {
            if (member is not FunctionSymbol function)
            {
                break;
            }

            overloads ??= new Dictionary<OverloadSignature, List<Symbol>>();
            var signature = OverloadSignature.Create(function);
            var list = overloads.GetOrAdd(signature, () => []);
            list.Add(function);
        }

        if (overloads is null)
        {
            ReportDuplicateDefinitions(name, members.AsSpan(), context);
        }
        else
        {
            foreach (var (_, functions) in overloads)
            {
                if (functions.Count == 1)
                    continue;

                ReportDuplicateDefinitions(name, CollectionsMarshal.AsSpan(functions), context);
            }
        }
    }

    private static void ReportDuplicateDefinitions(
        string name,
        ReadOnlySpan<Symbol> members,
        BindingContext context
    )
    {
        var primaryMemberLocations = members[0].Locations.AsSpan();
        var primaryLocation = primaryMemberLocations[0];
        context.ReportDiagnostic(
            Diagnostic.DuplicateDeclaration(
                primaryLocation,
                [
                    .. primaryMemberLocations[1..],
                    .. members[1..]
                        .AsValueEnumerable()
                        .SelectMany(x => x.Locations.AsValueEnumerable()),
                ],
                name
            )
        );
    }
}
