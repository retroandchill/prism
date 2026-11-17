using Prism.Compiler;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Tests;

public class TypeLayoutTests
{
    private CompilationContext _context;

    [SetUp]
    public void CreateContext()
    {
        _context = CompilationContext.CreateTestContext();
    }

    [TearDown]
    public void DisposeContext()
    {
        _context.Dispose();
    }

    [Test]
    public void NullableSpansAreStillWidePointers()
    {
        var tree = SyntaxTree.Parse("func foo(msg: str&?) { }");

        const string assemblyName = "test";
        var compilation = Compilation.Create(_context, assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("foo");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());
        var foo = (FunctionSymbol)members[0];

        Assert.That(foo.Parameters, Has.Length.EqualTo(1));
        var param = foo.Parameters[0];
        var paramType = param.Type;

        var layout = compilation.GetTypeLayout(paramType);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(layout.Size, Is.EqualTo(16));
            Assert.That(layout.Alignment, Is.EqualTo(8));
        }
    }
}
