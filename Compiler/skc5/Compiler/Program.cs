using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using System.IO;
using System.CodeDom.Compiler;
using Mirrored.SharpKit.JavaScript;
using System.Diagnostics;
using System.Configuration;
using System.Threading;
using System.Globalization;
using Corex.IO.Tools;

namespace SharpKit.Compiler
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("No arguments!");
                return 0;
            }

            string[] resolvedArgs = null;
            string paramFileTag = "/paramFile:";
            if (args.Length == 1 && args[0].StartsWith(paramFileTag))
            {
                string paramFile = args[0].Replace(paramFileTag,"");
                if (File.Exists(paramFile))
                {
                    string longArgs = File.ReadAllText(paramFile);
                    var tokenizer = new ToolArgsTokenizer();
                    resolvedArgs = tokenizer.Tokenize(longArgs);
                }
                else
                {
                    System.Console.WriteLine("Error:<{0}> is not found", paramFile);
                    return 0;
                }
            }

            if (resolvedArgs == null)
            {
                resolvedArgs = args;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            CollectionExtensions.Parallel = ConfigurationManager.AppSettings["Parallel"] == "true";
            CollectionExtensions.ParallelPreAction = () => Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //Console.AutoFlush = true;
            System.Console.WriteLine("Parallel=" + CollectionExtensions.Parallel);
            var skc = new CompilerTool { CommandLineArguments = resolvedArgs };
            skc.Init();
#if DEBUG
            skc.Debug = true;
#endif
            var res = skc.Run();
            stopwatch.Stop();
            System.Console.WriteLine("Total: {0}ms", stopwatch.ElapsedMilliseconds);
            //System.Console.Flush();
            if (res < 0 && skc.Args.exitReadKey.GetValueOrDefault())
            {
                System.Console.ReadKey();
            }
            return res;

        }

    }


}
