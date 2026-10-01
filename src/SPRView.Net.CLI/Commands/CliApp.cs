using CliFx;

namespace SPRView.Net.CLI.Commands;

/// <summary>
/// Builds and runs the command line application.
/// </summary>
public static class CliApp
{
    public static Task<int> Run(string[] args) => new CliApplicationBuilder()
        .AddCommand<ThumbnailCommand>()
        .AddCommand<SaveImageCommand>()
        .AddCommand<InformationCommand>()
        .AddCommand<PreviewCommand>()
        .AddCommand<CreateCommand>()
        .Build()
        .RunAsync(args)
        .AsTask();
}
