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

            string longArgs = File.ReadAllText(args[0]);
            //string longArgs = File.ReadAllText("D:/Code/NOVA/trunk/Program/JSBinding/Assets/Temp/skc_args.txt");
            string[] arr = longArgs.Split(' ');

            List<string> lst = new List<string>();
            int S = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                string s = arr[i];
                if (string.IsNullOrEmpty(s))
                    continue;

                if (S == 0)
                {
                    int a = s.IndexOf('\"');
                    int b = s.LastIndexOf('\"');
                    if (a >= 0 && b <= a)
                    {
                        lst.Add(s.Replace("\"", ""));
                        S = 1;
                    }
                    else
                    {
                        lst.Add(s.Replace("\"", ""));
                    }
                }
                else if (S == 1)
                {
                    if (s.IndexOf('\"') >= 0)
                    {
                        lst[lst.Count - 1] += " " + s.Replace("\"", "");
                        S = 0;
                    }
                    else
                    {
                        lst[lst.Count - 1] += " " + s;
                    }
                }
            }

            const string K1 = "/AllInvocationsOutput:";
            const string K2 = "/AllInvocationsWithLocationOutput:";
            const string K3 = "/YieldReturnTypeOutput:";
            foreach (var l in lst)
            {
                if (l.StartsWith(K1))
                    qiucw.InvocationOutputFile = l.Substring(K1.Length);
                else if (l.StartsWith(K2))
                    qiucw.InvocationOutputWithLocationFile = l.Substring(K2.Length);
                else if (l.StartsWith(K3))
                    qiucw.YieldReturnTypeFile = l.Substring(K3.Length);
            }

            //StringBuilder sb = new StringBuilder();
            //foreach (var l in lst)
            //    sb.AppendLine(l);
            //File.WriteAllText("D:\\6.txt", sb.ToString());

            //return 0;

            string[] resolvedArgs = lst.ToArray();


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
            //System.Console.Read();
            return res;

        }

    }


}
