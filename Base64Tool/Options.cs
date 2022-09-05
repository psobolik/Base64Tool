using CommandLine;
using CommandLine.Text;

namespace Base64Tool
{
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
