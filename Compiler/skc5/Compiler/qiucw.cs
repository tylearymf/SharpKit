using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirrored.SharpKit.JavaScript;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Extensions;

namespace SharpKit.Compiler
{
    public static class qiucw
    {
        public class InvocationLocation
        {
            public string FileName;
            public int Line;
        }

        // TypeName -> (MethodName -> List Of Locations)
        public static Dictionary<string, Dictionary<string, List<InvocationLocation>>> dictInvocation =
            new Dictionary<string, Dictionary<string, List<InvocationLocation>>>();

        // 看 CsExternalMetadata.Process
        static HashSet<string> typeDefaultIsExported = new HashSet<string>
        {
            "System.String",
            "System.Array",
            "System.Object",
            "System.Byte",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Decimal",
            "System.Single",
            "System.Double",
            "System.Type",
            "System.Delegate",
            "System.MulticastDelegate"
        };
        static void AddInvocation(string typeFullName, string methodName, InvocationLocation Loc)
        {
            Dictionary<string, List<InvocationLocation>> D;
            if (!dictInvocation.TryGetValue(typeFullName, out D))
            {
                D = new Dictionary<string, List<InvocationLocation>>();
                dictInvocation.Add(typeFullName, D);
            }

            List<InvocationLocation> L;
            if (!D.TryGetValue(methodName, out L))
            {
                L = new List<InvocationLocation>();
                D.Add(methodName, L);
            }

            L.Add(Loc);
        }
        public static void CheckAddInvocation(ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult res, string methodName)
        {
            var member = res.Member;
            string typeFullName = SkJs.GetEntityJsName(member.DeclaringType);

            bool exported = Sk.IsJsExported(member);

            // 如果是调用 Framework 里的函数
            if (!exported || typeDefaultIsExported.Contains(typeFullName))
            {
                InvocationLocation Loc = null;
                var firstNode = res.GetFirstNode();
                if (firstNode != null)
                    Loc = new InvocationLocation { FileName = firstNode.GetFileName(), Line = firstNode.StartLocation.Line };
                else if (InvokeRR2Location.ContainsKey(res))
                    Loc = InvokeRR2Location[res];

                if (Loc != null)
                {
                    if (member.IsStatic)
                        methodName = "Static_" + methodName;
                    AddInvocation(typeFullName, methodName, Loc);
                }
            }
        }
        public static void CheckAddInvocation(ICSharpCode.NRefactory.Semantics.MemberResolveResult res, string methodName)
        {
            var member = res.Member;
            string typeFullName = SkJs.GetEntityJsName(member.DeclaringType);
            bool exported = Sk.IsJsExported(member);
            if (!exported || typeDefaultIsExported.Contains(typeFullName))
            {
                InvocationLocation Loc = null;
                var firstNode = res.GetFirstNode();
                if (firstNode != null)
                    Loc = new InvocationLocation { FileName = firstNode.GetFileName(), Line = firstNode.StartLocation.Line };

                if (Loc != null)
                {
                    if (member.IsStatic)
                        methodName = "Static_" + methodName;
                    AddInvocation(typeFullName, methodName, Loc);
                }
            }
        }

        public static Dictionary<ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult, InvocationLocation> InvokeRR2Location =
            new Dictionary<ICSharpCode.NRefactory.CSharp.Resolver.CSharpInvocationResolveResult, InvocationLocation>();


        static Dictionary<string, List<InvocationLocation>> YieldType2Location = new Dictionary<string,List<InvocationLocation>>();
        public static void AddYieldReturn(ICSharpCode.NRefactory.CSharp.YieldReturnStatement node)
        {
            var rr = node.Expression.Resolve() as ICSharpCode.NRefactory.Semantics.ConversionResolveResult;
            try
            {
                if (rr != null)
                {
                    string k;
                    if (rr.Input.Type.Kind == TypeKind.Null)
                        k = "null";
                    else
                        k = SkJs.GetEntityJsName(rr.Input.Type);

                    InvocationLocation loc = new InvocationLocation { FileName = node.GetFileName(), Line = node.StartLocation.Line };
                    if (!YieldType2Location.ContainsKey(k))
                        YieldType2Location.Add(k, new List<InvocationLocation> { loc });
                    else
                        YieldType2Location[k].Add(loc);
                }
            }
            catch (Exception e)
            {
                e = e;
            }
        }


        public static string InvocationOutputFile { get; set; }
        public static string InvocationOutputWithLocationFile { get; set; }
        public static string YieldReturnTypeFile { get; set; }

        public static void OutputToFile()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var KV in qiucw.dictInvocation)
            {
                sb.AppendFormat("[{0}]\r\n", KV.Key);

                Dictionary<string, List<qiucw.InvocationLocation>> D = KV.Value;
                foreach (var KV2 in D)
                {
                    sb.AppendFormat("    {0}\r\n", KV2.Key);
                }
                sb.Append("\r\n");
            }
            File.WriteAllText(InvocationOutputFile, sb.ToString());

            //------------------------------------------------------------------------------

            sb = new StringBuilder();
            foreach (var KV in qiucw.dictInvocation)
            {
                sb.AppendFormat("[{0}]\r\n", KV.Key);

                Dictionary<string, List<qiucw.InvocationLocation>> D = KV.Value;
                foreach (var KV2 in D)
                {
                    sb.AppendFormat("    {0}\r\n", KV2.Key);

                    List<qiucw.InvocationLocation> L = KV2.Value;
                    for (int i = 0; i < L.Count; i++)
                    {
                        sb.AppendFormat("        {0},{1},{2}\r\n", i + 1, L[i].FileName, L[i].Line);
                    }
                }
                sb.Append("\r\n");
            }
            File.WriteAllText(InvocationOutputWithLocationFile, sb.ToString());

            //----------------------------------------------------------------------------

            sb = new StringBuilder();
            foreach (var KV in YieldType2Location)
            {
                sb.AppendFormat("[{0}]\r\n", KV.Key);

                List<qiucw.InvocationLocation> L = KV.Value;
                for (int i = 0; i < L.Count; i++)
                {
                    sb.AppendFormat("    {0},{1},{2}\r\n", i + 1, L[i].FileName, L[i].Line);
                }
            }
            File.WriteAllText(YieldReturnTypeFile, sb.ToString());
        }
    }
}
