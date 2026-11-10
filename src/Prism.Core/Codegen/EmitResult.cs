using System.Collections.Immutable;
using Prism.Diagnostics;

namespace Prism.Core.Codegen;

public readonly record struct EmitResult(bool Success, ImmutableArray<Diagnostic> Diagnostics);
