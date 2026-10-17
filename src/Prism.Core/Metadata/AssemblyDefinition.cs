// @file AssemblyDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinition(AssemblyIdentity identity, TargetInfo target)
    : AssemblyDescriptor(identity, new MetadataToken(TokenType.Assembly, 1))
{
    private bool _isFrozen;

    public TargetInfo Target { get; } = target;

    private readonly List<AssemblyReference> _referencedAssemblies = [];

    public IReadOnlyList<AssemblyReference> ReferencedAssemblies => _referencedAssemblies;

    public void AddAssemblyReference(AssemblyReference reference)
    {
        ThrowIfFrozen();
        _referencedAssemblies.Add(reference);
    }

    public void Freeze()
    {
        _isFrozen = true;
    }

    private void ThrowIfFrozen()
    {
        if (_isFrozen)
            throw new InvalidOperationException("Metadata definition is frozen.");
    }
}
