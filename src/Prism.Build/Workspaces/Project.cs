using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Build.Manifest;
using Prism.Core.Codegen;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Syntax;
using Tomlyn;
using ZLinq;

namespace Prism.Build.Workspaces;

public union ProjectResult(Project, string);

public sealed class Project
{
    private readonly ProjectManifest _manifest;
    private readonly ImmutableArray<FileInfo> _sourceFiles;
    private Compilation? _compilation;
    private readonly DirectoryInfo _buildDir;

    private Project(
        ProjectManifest manifest,
        ImmutableArray<FileInfo> srcFiles,
        DirectoryInfo buildDir
    )
    {
        _manifest = manifest;
        _sourceFiles = srcFiles;
        _buildDir = buildDir;
    }

    public static ProjectResult FromDirectory(DirectoryInfo directory)
    {
        var file = new FileInfo(Path.Combine(directory.FullName, "facet.toml"));
        if (!file.Exists)
        {
            return "No facet.toml file found";
        }

        ProjectManifest manifest;
        using (var reader = file.OpenText())
        {
            manifest =
                TomlSerializer.Deserialize<ProjectManifest>(reader, PrismTomlContext.Default)
                ?? throw new TomlException("Failed to deserialize facet.toml");
        }

        var srcDir = new DirectoryInfo(Path.Combine(directory.FullName, "src"));
        if (!srcDir.Exists)
        {
            return "No src directory found";
        }

        var srcFiles = srcDir.GetFiles("*.pr", SearchOption.AllDirectories);
        if (srcFiles.Length == 0)
        {
            return "No .pr files found";
        }

        var buildDir = new DirectoryInfo(Path.Combine(directory.FullName, "build"));
        return new Project(
            manifest,
            ImmutableCollectionsMarshal.AsImmutableArray(srcFiles),
            buildDir
        );
    }

    public Compilation GetCompilation(CancellationToken cancellationToken)
    {
        if (_compilation is not null)
            return _compilation;

        Interlocked.CompareExchange(ref _compilation, ComputeCompilation(cancellationToken), null);
        return _compilation;
    }

    private Compilation ComputeCompilation(CancellationToken cancellationToken)
    {
        var trees = ComputeSyntaxTrees(cancellationToken);
        var settings = CompilationSettings.CurrentPlatform with
        {
            OutputKind = _manifest.Output.Kind,
            BuildingCoreLibrary = _manifest.Package.IsCoreLibrary,
        };
        return Compilation.Create(_manifest.Package.Name, trees, settings);
    }

    private ImmutableArray<SyntaxTree> ComputeSyntaxTrees(CancellationToken cancellationToken)
    {
        var trees = new SyntaxTree[_sourceFiles.Length];
        foreach (var (i, file) in _sourceFiles.AsValueEnumerable().Index())
        {
            cancellationToken.ThrowIfCancellationRequested();
            string fileText;
            using (var reader = file.OpenText())
            {
                fileText = reader.ReadToEnd();
            }

            trees[i] = SyntaxTree.Parse(file.Name, fileText);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(trees);
    }

    public async Task<EmitResult> BuildAsync(CancellationToken cancellationToken)
    {
        var compilation = GetCompilation(cancellationToken);
        if (!_buildDir.Exists)
        {
            _buildDir.Create();
        }

        return await compilation.EmitAsync(_buildDir.FullName, cancellationToken);
    }
}
