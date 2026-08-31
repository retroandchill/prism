using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class SymbolCreationTest
{
    [Test]
    public void DeclarationMerging()
    {
        var tree = SyntaxTree.Parse(
            """
            namespace A::B {
                var x: i32 = 5;
            }

            namespace A {
                namespace B {
                    func f(p: i32) {}
                }
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers();
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("A"));

        Assert.That(members[0], Is.InstanceOf<NamespaceSymbol>());
        members = ((NamespaceSymbol)members[0]).GetMembers();
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0].Name, Is.EqualTo("B"));
        Assert.That(members[0], Is.InstanceOf<NamespaceSymbol>());

        members = ((NamespaceSymbol)members[0]).GetMembers();

        Assert.That(members, Has.Length.EqualTo(2));
        Assert.That(members[0].Name, Is.EqualTo("x"));
        Assert.That(members[0], Is.InstanceOf<VariableSymbol>());
        Assert.That(members[1].Name, Is.EqualTo("f"));
        Assert.That(members[1], Is.InstanceOf<FunctionSymbol>());

        var x = ((VariableSymbol)members[0]).Type;
        Assert.That(x.SpecialType, Is.EqualTo(SpecialType.I32));

        var f = ((FunctionSymbol)members[1]).ReturnType;
        Assert.That(f.SpecialType, Is.EqualTo(SpecialType.Void));

        var p = ((FunctionSymbol)members[1]).Parameters[0].Type;
        Assert.That(p.SpecialType, Is.EqualTo(SpecialType.I32));
    }
}
