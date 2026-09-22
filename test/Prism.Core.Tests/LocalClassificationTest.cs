using System.Collections.Immutable;
using Prism.Core.Codegen.Mir;
using Prism.Core.Compiling;
using Prism.Core.Mir.Analysis;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Tests;

public class LocalClassificationTest
{
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
        var compilation = Compilation.Create(assemblyName, [tree]);

        var members = compilation.Assembly.GlobalNamespace.GetMembers("nested_break_and_continue");
        Assert.That(members, Has.Length.EqualTo(1));
        Assert.That(members[0], Is.InstanceOf<FunctionSymbol>());

        var function = (FunctionSymbol)members[0];
        var mirEmitter = new MirEmitter(compilation);
        var mirFunction = mirEmitter.EmitFunction(function, CancellationToken.None);

        var cfg = MirFunctionAnalyzer.AnalyzeControlFlow(mirFunction, CancellationToken.None);
        var localFlow = MirFunctionAnalyzer.AnalyzeLocalFlow(
            mirFunction,
            cfg,
            CancellationToken.None
        );
        var classifiedLocals = MirFunctionAnalyzer.ClassifyLocals(
            compilation,
            mirFunction,
            localFlow,
            CancellationToken.None
        );

        var allocatedLocals = classifiedLocals
            .Locals.Where(l => l.Value.StorageKind == MirLocalStorageKind.Memory)
            .ToImmutableArray();

        Assert.That(allocatedLocals, Has.Length.EqualTo(2));
    }
}
