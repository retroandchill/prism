// @file SingleTypeDeclaration.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Declarations;

internal sealed class SingleTypeDeclaration : SingleDeclaration
{
    public SingleTypeDeclaration(
        DeclarationKind kind,
        string name,
        SyntaxReference syntaxReference,
        DeclarationModifiers modifiers,
        SourceLocation nameLocation,
        ImmutableHashSet<string> memberNames,
        ImmutableArray<SingleTypeDeclaration> members
    )
        : base(name, syntaxReference, nameLocation, memberNames)
    {
        Debug.Assert(kind != DeclarationKind.Namespace);
        Kind = kind;
        Members = members;
        Modifiers = modifiers;
    }

    public override DeclarationKind Kind { get; }

    public TypeDeclarationIdentity Identity => new(this);

    public new ImmutableArray<SingleTypeDeclaration> Members { get; }

    public DeclarationModifiers Modifiers { get; }

    protected override ImmutableArray<SingleDeclaration> GetSingleMembers()
    {
        return ImmutableArray<SingleDeclaration>.CastUp(Members);
    }

    internal readonly struct TypeDeclarationIdentity
        : IEquatable<TypeDeclarationIdentity>,
            IEqualityOperators<TypeDeclarationIdentity, TypeDeclarationIdentity, bool>
    {
        private readonly SingleTypeDeclaration _decl;

        internal TypeDeclarationIdentity(SingleTypeDeclaration decl)
        {
            _decl = decl;
        }

        public override bool Equals(object? obj)
        {
            return obj is TypeDeclarationIdentity other && _decl.Equals(other._decl);
        }

        public bool Equals(TypeDeclarationIdentity other)
        {
            if (_decl == other._decl)
                return true;

            if (_decl.Kind != other._decl.Kind || _decl.Name != other._decl.Name)
                return false;

            if (
                _decl.SyntaxReference.SyntaxTree != other._decl.SyntaxReference.SyntaxTree
                && (
                    _decl.Modifiers.HasFlag(DeclarationModifiers.File)
                    || (other._decl.Modifiers.HasFlag(DeclarationModifiers.File))
                )
            )
                return false;

            // Attributes cannot be partial
            if (_decl.Kind is DeclarationKind.Attribute)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_decl.Kind, _decl.Name);
        }

        public static bool operator ==(TypeDeclarationIdentity left, TypeDeclarationIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypeDeclarationIdentity left, TypeDeclarationIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
