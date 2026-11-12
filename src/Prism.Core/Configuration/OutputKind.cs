using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Prism.Compiler.Configuration;

public enum OutputKind : byte
{
    Executable,
    StaticLibrary,
    SharedLibrary,
}
