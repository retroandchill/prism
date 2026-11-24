using Prism.Compiler;
using Prism.Configuration;
using Prism.Semantic.Symbols;
using Prism.Syntax;
using Prism.Tests.Helpers;

namespace Prism.Tests;

public class TypeAliasTests
{
    private CompilationContext _context;

    private static readonly CompilationSettings Settings = new()
    {
        OutputKind = OutputKind.StaticLibrary,
    };

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
    public void TypeAliasesAreEquivalentToTheirUnderlyingType()
    {
        var tree = SyntaxTree.Parse("type t = i32;");

        const string assemblyName = "test";
        var compilation = Compilation.Create(
            _context,
            assemblyName,
            [tree],
            TestHelpers.TestReferences,
            Settings
        );

        var members = compilation.Assembly.GlobalNamespace.GetMembers("t");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<TypeAliasSymbol>());

        var typeAlias = (TypeAliasSymbol)members[0];
        Assert.That(typeAlias, Is.EqualTo(typeAlias.AliasedType));
    }

    [Test]
    public void TypeAliasesCanBeFreelyAssignedIntoItself()
    {
        var tree = SyntaxTree.Parse(
            """
            type T = i32;

            func identity_test(value: i32): T {
                return value;
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(
            _context,
            assemblyName,
            [tree],
            TestHelpers.TestReferences,
            Settings
        );

        var diagnostics = compilation.GetDiagnostics();
        Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public void TypeAliasesAreInterchangable()
    {
        var tree = SyntaxTree.Parse(
            """
            type T = i32;
            type U = i32;
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(
            _context,
            assemblyName,
            [tree],
            TestHelpers.TestReferences,
            Settings
        );

        var members = compilation.Assembly.GlobalNamespace.GetMembers("T");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<TypeAliasSymbol>());

        var typeAlias1 = (TypeAliasSymbol)members[0];

        members = compilation.Assembly.GlobalNamespace.GetMembers("U");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<TypeAliasSymbol>());

        var typeAlias2 = (TypeAliasSymbol)members[0];

        Assert.That(typeAlias1, Is.EqualTo(typeAlias2));
    }
}
