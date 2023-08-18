using CommandLine;

namespace Base64Tool
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class Options
    {
        [Option('b', "break", HelpText = "Break encoded string at num columns")]
        public int BreakColumn { get; set; }

        [Option('D', "decode", HelpText = "Decode input")]
        public bool Decode { get; set; }

        [Option('i', "input", HelpText = @"Input file (default: ""-"" for stdin")]
        public string Input { get; set; }

        [Option('o', "output", HelpText = @"Output file (default: ""-"" for stdout")]
        public string Output { get; set; }
    }
}
