using Prism.Core.Compiling;
using Prism.Core.Syntax;

const string programPath = "D:/dev/prism/examples/hello_world/main.pr";

Console.WriteLine("Parsing Program");
string program;
using (var stream = File.OpenRead(programPath))
{
    using var reader = new StreamReader(stream);
    program = reader.ReadToEnd();
}

var syntaxTree = SyntaxTree.Parse(program);
var compilation = Compilation.Create("test", [syntaxTree]);

if (compilation.Emit(Directory.GetParent(programPath)!.FullName) is (false, var diagnostics))
{
    Console.WriteLine("Compilation Failed");
    foreach (var diagnostic in diagnostics)
        Console.WriteLine(diagnostic);
    return 1;
}

Console.WriteLine("Compilation Succeeded");
return 0;
