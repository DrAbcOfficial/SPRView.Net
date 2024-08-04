using CliFx;
namespace SPRView.Net.Cli;
public class CLICommandManager
{
    public async void Run(string[] args) => await new CliApplicationBuilder()
        .AddCommand<ThumbnailCommand>()
        .AddCommand<SaveImageCommand>()
        .AddCommand<InformationCommand>()
        .Build()
        .RunAsync(args);
}