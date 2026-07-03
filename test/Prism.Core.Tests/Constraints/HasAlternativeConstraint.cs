// @file HasAlternativeContraint.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using NUnit.Framework.Constraints;

namespace Prism.Core.Tests.Constraints;

public class HasAlternativeConstraint<TConstraint> : Constraint
{
    public override string Description =>
        "Checks that a union has an alternative of type TConstraint";

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        return actual is IUnion { Value: TConstraint }
            ? new ConstraintResult(this, actual, true)
            : new ConstraintResult(this, actual, false);
    }
}

public static class NUnitExtensions
{
    extension(Has)
    {
        public static HasAlternativeConstraint<TConstraint> Alternative<TConstraint>()
        {
            return new HasAlternativeConstraint<TConstraint>();
        }
    }
}
