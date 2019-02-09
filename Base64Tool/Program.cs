using CommandLine;
using System;

namespace Base64Tool
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArgumentsStrict(args, options)) return;

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