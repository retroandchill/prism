// @file SourcePosition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using ReflectionIT.ComparisonOperatorsGenerator.Attributes;

namespace Prism.Core.Text;

[ComparisonOperators(ImplementIComparisonOperatorsInterface = true)]
public readonly partial record struct SourcePosition(int Line, int Column)
    : IComparable<SourcePosition>
{
    public int CompareTo(SourcePosition other)
    {
        var lineComparison = Line.CompareTo(other.Line);
        return lineComparison != 0 ? lineComparison : Column.CompareTo(other.Column);
    }

    public override string ToString()
    {
        return $"{Line}:{Column}";
    }
}
