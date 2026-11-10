using Prism.Core.Metadata;

namespace Prism.Core.Diagnostics;

public interface IMetadataReferenceInfo
{
    AssemblyIdentity Identity { get; }
}
