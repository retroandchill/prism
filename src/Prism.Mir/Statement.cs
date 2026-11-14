// @file Statement.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Mir;

public closed record Statement
{
    public abstract override string ToString();
}

public sealed record AssignmentStatement(Place Target, RValue Value) : Statement
{
    public override string ToString() => $"{Target} = {Value}";
}

public sealed record StorageLiveStatement(Local Local) : Statement
{
    public override string ToString() => $"Live({Local})";
}

public sealed record StorageDeadStatement(Local Local) : Statement
{
    public override string ToString() => $"Dead({Local})";
}

public sealed record MakeNullStatement(Place Place) : Statement
{
    public override string ToString() => $"MakeNull({Place})";
}
