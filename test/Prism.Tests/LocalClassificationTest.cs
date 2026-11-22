using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Codegen.Mir;
using Prism.Compiler;
using Prism.Mir.Analysis;
using Prism.Semantic.Symbols;
using Prism.Syntax;
using Prism.Tests.Helpers;

namespace Prism.Tests;

public class LocalClassificationTest
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
    public void BooleanConditionsInLoopsShouldNotCauseAllocas()
    {
        var tree = SyntaxTree.Parse(
            """
            func nested_break_and_continue() {
                outer: for (mutable var i = 0; i < 10; i++) {
                    for (mutable var j = 0; j < 10; j++) {
                        if (i == 5 && j == 5) {
                            continue outer;
                        }

                        if (i == 5 && j == 7) {
                            break outer;
                        }
                    }
                }
            }
            """
        );

        const string assemblyName = "test";
        var compilation = Compilation.Create(
            _context,
            assemblyName,
            [tree],
            TestHelpers.TestReferences
        );

        var members = compilation.Assembly.GlobalNamespace.GetMembers("nested_break_and_continue");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var mirEmitter = new MirEmitter(compilation);
        var mirFunction = mirEmitter.Emit(function, CancellationToken.None);
        Assert.That(mirFunction.Value, Is.Not.Null);

        var classifiedLocals = LocalClassification.GetClassifications(
            compilation,
            mirFunction.Value,
            CancellationToken.None
        );

        var allocatedLocals = classifiedLocals
            .Where(l => l.Value.StorageKind == LocalStorageKind.StackSlot)
            .ToImmutableArray();

        Assert.That(allocatedLocals, Has.Length.EqualTo(4));
    }
}
