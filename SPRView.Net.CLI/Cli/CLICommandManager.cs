using CliFx;

namespace SPRView.Net.CLI.Cli;
public class CLICommandManager
{
    public async void Run(string[] args) => await new CliApplicationBuilder()
        .AddCommand<ThumbnailCommand>()
        .AddCommand<SaveImageCommand>()
        .AddCommand<InformationCommand>()
        .AddCommand<PreviewCommand>()
        .AddCommand<CreateCommand>()
        .Build()
        .RunAsync(args);
}