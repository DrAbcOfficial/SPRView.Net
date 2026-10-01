namespace SPRView.Net.Thumbnailer;

/// <summary>
/// Parsed command line for the thumbnailer.
/// </summary>
/// <remarks>
/// Accepts both invocation styles used in the wild:
/// <c>sprview-thumbnailer input output</c> (XDG thumbnailer spec, %i %o) and
/// <c>sprview-thumbnailer -s SIZE input output</c> (legacy gnome style).
/// </remarks>
internal sealed class ThumbnailerOptions
{
    public required string InputPath { get; init; }
    public required string OutputPath { get; init; }
    public int MaxSize { get; init; } = 256;

    public static bool TryParse(string[] args, out ThumbnailerOptions options)
    {
        options = null!;

        int maxSize = 256;
        List<string> positional = [];
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg is "-s" or "--size")
            {
                if (i + 1 >= args.Length || !int.TryParse(args[++i], out maxSize) || maxSize <= 0)
                    return false;
            }
            else if (arg.StartsWith("--size=", StringComparison.Ordinal))
            {
                if (!int.TryParse(arg["--size=".Length..], out maxSize) || maxSize <= 0)
                    return false;
            }
            else if (arg is "-h" or "--help")
            {
                return false;
            }
            else
            {
                positional.Add(arg);
            }
        }

        if (positional.Count != 2)
            return false;

        options = new ThumbnailerOptions
        {
            InputPath = positional[0],
            OutputPath = positional[1],
            MaxSize = maxSize
        };
        return true;
    }
}
