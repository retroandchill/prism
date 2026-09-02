using Prism.Core.Compiling;
using Prism.Core.Syntax;

const string programPath = "D:/dev/prism/examples/hello_world/main.pr";

string program;
using (var stream = File.OpenRead(programPath))
{
    using var reader = new StreamReader(stream);
    program = reader.ReadToEnd();
}

var syntaxTree = SyntaxTree.Parse(program);
var compilation = Compilation.Create("test", [syntaxTree]);

if (compilation.Emit(Directory.GetParent(programPath)!.FullName) is (false, _))
{
    Console.WriteLine("Compilation Failed");
    return 1;
}

return 0;
