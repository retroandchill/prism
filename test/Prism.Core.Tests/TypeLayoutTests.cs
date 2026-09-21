using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class TypeLayoutTests
{
    [Test]
    public void NullableSpansAreStillWidePointers()
    {
        var tree = SyntaxTree.Parse("func foo(msg: str&?) { }");

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

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
