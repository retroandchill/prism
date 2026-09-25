using DotMake.CommandLine;
using Prism.Build.Manifest;

namespace Prism.Facet;

[CliCommand(Description = "Invoke the Facet build system")]
public class RootCommand
{
    [CliCommand(Description = "Build the project")]
    public class BuildCommand
    {
        [CliArgument(
            Description = "The folder to build, leave empty to build the current folder",
            Required = false
        )]
        public DirectoryInfo? Folder { get; set; } = null;

        public async Task<int> RunAsync(CliContext context)
        {
            var folder = Folder ?? new DirectoryInfo(Directory.GetCurrentDirectory());

            var file = folder.GetFiles("facet.toml").SingleOrDefault();
            if (file is null)
            {
                await context.Error.WriteLineAsync("No facet.toml file found");
                return 1;
            }

            ProjectManifest manifest;
            await using (var stream = file.OpenRead())
            {
                manifest = ProjectManifest.Read(stream);
            }

            await context.Output.WriteLineAsync(manifest.ToString());

            return 0;
        }
    }
}
