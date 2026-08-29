using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Codegen;

public readonly record struct EmitResult(bool Success, ImmutableArray<Diagnostic> Diagnostics);
