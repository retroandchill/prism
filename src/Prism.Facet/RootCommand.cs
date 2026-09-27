using DotMake.CommandLine;
using Prism.Build.Workspaces;

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
        // ReSharper disable once RedundantDefaultMemberInitializer
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

            var projectResult = Project.FromDirectory(folder);
            return projectResult switch
            {
                Project project => await BuildProjectAsync(project, context),
                string error => await ReportProjectError(error, context),
            };
        }

        private static async Task<int> BuildProjectAsync(Project project, CliContext context)
        {
            var (success, diagnostics) = await project.BuildAsync(context.CancellationToken);

            if (!success)
            {
                await context.Output.WriteLineAsync("Build failed");
            }
            else
            {
                await context.Output.WriteLineAsync("Build succeeded");
            }

            foreach (var diagnostic in diagnostics)
            {
                await context.Output.WriteLineAsync(diagnostic.ToString());
            }

            return success ? 0 : 1;
        }

        private static async ValueTask<int> ReportProjectError(string msg, CliContext context)
        {
            await context.Output.WriteLineAsync(msg);
            return 1;
        }
    }
}
