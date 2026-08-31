using DotMake.CommandLine;
using Prism.SyntaxGenerator;
using Prism.SyntaxGenerator.Commands;

await Cli.RunAsync<RootCommand>(args);
