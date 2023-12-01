using QuarterMaster.Communications.Rest.COM;

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuarterMaster.Logging
{
    public static class BlueLog
    {
        private static readonly Stack<string> ModuleQueue = new Stack<string>();
        private static readonly string ProgramName = System.IO.Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

        //private BlueLog()
        //{
        //    ModuleQueue.Enqueue("");
        //}

        public static void Module(params object[] args)
        {
            if (args.Length == 1)
            {
                ModuleQueue.Push(args[0].ToString());
            }
            else
            {
                if (ModuleQueue.LongCount() > 1)
                {
                    ModuleQueue.Pop();
                }
            }
        }

        public static void PushModule(string moduleName)
        {
            ModuleQueue.Push(moduleName);
        }

        public static void PopModule()
        {
            if (ModuleQueue.LongCount() > 0)
            {
                ModuleQueue.Pop();
            }
        }

        public static void Information(object msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "I", msg);
        }

        public static void Warning(object msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "W", msg);
        }

        public static void Error(object msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "E", msg);
        }

        public static void Information(params object[] msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "I", string.Join(" ", msg));
        }

        public static void Warning(params object[] msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "W", string.Join(" ", msg));
        }

        public static void Error(params object[] msg)
        {
            JLog(ProgramName, string.Join<string>(".", ModuleQueue.ToArray<string>().Reverse<string>()), "E", string.Join(" ", msg));
        }

        private static void JLog(string program, string module, string level, object msg)
        {
            RESTful R = new RESTful("https://blue.q-metrics.com.au")
                .Path("/JLog.ashx")
                .Verb("POST")
                .Tail($"p={program}")
                .Tail($"m={module}")
                .Tail($"l={level.Substring(0, 1).ToUpper()}")
                .Body($"{msg}")
                .UserAgent("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36")
                .Send();
        }
    }
}
