using System;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace Base64Tool
{
    static class Program
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class Options
        {
            [Option('b', "break", HelpText = "Break encoded string at num columns")]
            public int BreakColumn { get; set; }

            [Option('D', "decode", HelpText = "Decode input")]
            public bool Decode { get; set; }

            [Option('i', "input", HelpText = @"Input file (default: ""-"" for stdin")]
            public string Input { get; set; }

            [Option('o', "output", HelpText = @"Output file (default: ""-"" for stdout")]
            public string Output { get; set; }

            [HelpOption]
            // ReSharper disable once UnusedMember.Local
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }        
        }

        [STAThread]
        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArgumentsStrict(args, options)) 
            {
                try
                {
                    if (options.Decode)
                    {
                        Base64Helper.Decode(options.Input, options.Output);
                    }
                    else
                    {
                        Base64Helper.Encode(options.Input, options.Output, options.BreakColumn);
                    }
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }
    }
}