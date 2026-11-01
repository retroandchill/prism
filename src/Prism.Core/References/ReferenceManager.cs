using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Symbols;

namespace Prism.Core.References;

internal union AssemblyOrDiagnostic(AssemblyMetadata, Diagnostic);

internal sealed class ReferenceManager { }
