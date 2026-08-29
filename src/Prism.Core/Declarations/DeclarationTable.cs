using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Compiling;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Declarations;

internal sealed class DeclarationTable
{
    private readonly ImmutableOrderedSet<Lazy<SingleRootNamespaceDeclaration>> _oldRoots;
    private readonly Lazy<SingleRootNamespaceDeclaration>? _latestRoot;
    private Cache _cache;
    private MergedNamespaceDeclaration? _mergedRoot;

    private DeclarationTable(
        ImmutableOrderedSet<Lazy<SingleRootNamespaceDeclaration>> oldRoots,
        Lazy<SingleRootNamespaceDeclaration>? lazyRoot,
        Cache? cache
    )
    {
        _oldRoots = oldRoots;
        _latestRoot = lazyRoot;
        _cache = cache ?? new Cache(this);
    }

    public static readonly DeclarationTable Empty = new(
        ImmutableOrderedSet<Lazy<SingleRootNamespaceDeclaration>>.Empty,
        null,
        null
    );

    public Builder ToBuilder() => new(this);

    public MergedNamespaceDeclaration GetMergedRoot(Compilation compilation)
    {
        if (_mergedRoot is not null)
            return _mergedRoot;

        Interlocked.CompareExchange(ref _mergedRoot, CalculateMergedRoot(compilation), null);
        return _mergedRoot;
    }

    public ImmutableHashSet<string> TypeNames
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, CalculateTypeNames(), null);
            return field;
        }
    }

    public ImmutableHashSet<string> NamespaceNames
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, CalculateNamespaceNames(), null);
            return field;
        }
    }

    private MergedNamespaceDeclaration CalculateMergedRoot(Compilation compilation)
    {
        var oldRoot = _cache.MergedRoot;
        if (_latestRoot is null)
            return oldRoot;

        var oldDeclarations = oldRoot.Declarations;
        var newDeclarations = new SingleNamespaceDeclaration[oldDeclarations.Length + 1];
        oldDeclarations.CopyTo(newDeclarations);
        newDeclarations[^1] = _latestRoot.Value;
        Array.Sort(newDeclarations, new NamespaceLocationComparer(compilation));
        return MergedNamespaceDeclaration.Create(
            ImmutableCollectionsMarshal.AsImmutableArray(newDeclarations)
        );
    }

    private sealed class NamespaceLocationComparer(Compilation compilation)
        : IComparer<SingleNamespaceDeclaration>
    {
        public int Compare(SingleNamespaceDeclaration? x, SingleNamespaceDeclaration? y)
        {
            if (x is null)
            {
                return y is null ? 0 : 1;
            }

            if (y is null)
            {
                return -1;
            }

            return compilation.CompareSourceLocations(x.SyntaxReference, y.SyntaxReference);
        }
    }

    private ImmutableHashSet<string> CalculateTypeNames()
    {
        var cachedNames = _cache.TypeNames;

        return _latestRoot is null
            ? cachedNames
            : cachedNames.Union(GetTypeNames(_latestRoot.Value));
    }

    private ImmutableHashSet<string> CalculateNamespaceNames()
    {
        var cachedNames = _cache.TypeNames;

        return _latestRoot is null
            ? cachedNames
            : cachedNames.Union(GetNamespaceNames(_latestRoot.Value));
    }

    private static ImmutableHashSet<string> GetTypeNames(Declaration declaration)
    {
        return GetNames(declaration, d => d.Kind != DeclarationKind.Namespace);
    }

    private static ImmutableHashSet<string> GetNamespaceNames(Declaration declaration)
    {
        return GetNames(declaration, d => d.Kind != DeclarationKind.Namespace);
    }

    private static ImmutableHashSet<string> GetNames(
        Declaration declaration,
        Predicate<Declaration> predicate
    )
    {
        var builder = ImmutableHashSet.CreateBuilder<string>();
        var stack = new Stack<Declaration>();
        stack.Push(declaration);

        while (stack.TryPop(out var current))
        {
            if (predicate(current))
            {
                builder.Add(current.Name);
            }

            foreach (var member in declaration.Members)
            {
                stack.Push(member);
            }
        }

        return builder.ToImmutable();
    }

    private sealed class Cache
    {
        private readonly ImmutableOrderedSet<Lazy<SingleRootNamespaceDeclaration>> _oldRoots;

        public Cache(DeclarationTable table)
        {
            _oldRoots = table._oldRoots;
        }

        public MergedNamespaceDeclaration MergedRoot
        {
            get
            {
                if (field is not null)
                    return field;

                var merged = MergedNamespaceDeclaration.Create(
                    _oldRoots
                        .AsValueEnumerable()
                        .Select(SingleNamespaceDeclaration (x) => x.Value)
                        .ToImmutableArray()
                );
                Interlocked.CompareExchange(ref field, merged, null);
                return field;
            }
        }

        public ImmutableHashSet<string> TypeNames
        {
            get
            {
                if (field is not null)
                    return field;

                Interlocked.CompareExchange(ref field, GetTypeNames(MergedRoot), null);
                return field;
            }
        }

        public ImmutableHashSet<string> NamespaceNames
        {
            get
            {
                if (field is not null)
                    return field;

                Interlocked.CompareExchange(ref field, GetNamespaceNames(MergedRoot), null);
                return field;
            }
        }
    }

    public sealed class Builder(DeclarationTable? table)
    {
        private DeclarationTable _table = table ?? Empty;
        private List<Lazy<SingleRootNamespaceDeclaration>>? _pendingAdds;
        private List<Lazy<SingleRootNamespaceDeclaration>>? _pendingRemoves;

        public void AddRootDeclaration(Lazy<SingleRootNamespaceDeclaration> declaration)
        {
            RealizeRemoves();
            _pendingAdds ??= [];
            _pendingAdds.Add(declaration);
        }

        public void RemoveRootDeclaration(Lazy<SingleRootNamespaceDeclaration> declaration)
        {
            RealizeAdds();
            _pendingRemoves ??= [];
            _pendingRemoves.Add(declaration);
        }

        public DeclarationTable Build()
        {
            RealizeAdds();
            RealizeRemoves();
            return _table;
        }

        private void RealizeAdds()
        {
            if (_pendingAdds is null || _pendingAdds.Count == 0)
            {
                return;
            }

            var lastDeclaration = _pendingAdds[^1];
            if (_pendingAdds.Count == 1)
            {
                if (_table._latestRoot is null)
                {
                    _table = new DeclarationTable(_table._oldRoots, lastDeclaration, _table._cache);
                }
                else
                {
                    _table = new DeclarationTable(
                        _table._oldRoots.Add(_table._latestRoot),
                        lastDeclaration,
                        _table._cache
                    );
                }
            }
            else
            {
                _pendingAdds.RemoveAt(_pendingAdds.Count - 1);

                if (_table._latestRoot is not null)
                {
                    _pendingAdds.Insert(0, _table._latestRoot);
                }

                var newOldRoots = _table._oldRoots.AddRange(_pendingAdds);
                _table = new DeclarationTable(newOldRoots, lastDeclaration, cache: null);
            }

            _pendingAdds.Clear();
        }

        private void RealizeRemoves()
        {
            if (_pendingRemoves is null || _pendingRemoves.Count == 0)
                return;

            if (_pendingRemoves.Count == 1)
            {
                var firstDeclaration = _pendingRemoves[0];
                if (_table._latestRoot == firstDeclaration)
                {
                    _table = new DeclarationTable(_table._oldRoots, null, _table._cache);
                }
                else
                {
                    _table = new DeclarationTable(
                        _table._oldRoots.Remove(firstDeclaration),
                        firstDeclaration,
                        _table._cache
                    );
                }
            }
            else
            {
                var isLatestRemoved =
                    _table._latestRoot is not null && _pendingRemoves.Contains(_table._latestRoot);

                var newOldRoots = _table._oldRoots.RemoveRange(_pendingRemoves);
                var newLatest = !isLatestRemoved ? _table._latestRoot : null;

                _table = new DeclarationTable(newOldRoots, newLatest, _table._cache);
            }

            _pendingRemoves.Clear();
        }
    }
}
