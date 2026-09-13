using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Prism.Core.Configuration;

public enum OutputKind : byte
{
    Executable,
    StaticLibrary,
    SharedLibrary,
}
