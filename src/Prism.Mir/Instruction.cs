// @file Statement.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Mir;

public closed record Instruction
{
    public abstract override string ToString();
}

public sealed record AssignmentInstruction(Place Target, RValue Value) : Instruction
{
    public override string ToString() => $"{Target} = {Value}";
}

public sealed record StorageLiveInstruction(Local Local) : Instruction
{
    public override string ToString() => $"Live({Local})";
}

public sealed record StorageDeadInstruction(Local Local) : Instruction
{
    public override string ToString() => $"Dead({Local})";
}
