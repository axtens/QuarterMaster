using QuarterMaster.Communications.Rest.COM;
using QuarterMaster.Debugging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuarterMaster.Logging
{
    public class AppLog
    {
        public R1V LastMessage { get; set; }
        //private static readonly string message;
        private enum LEVELS { DEBUG, INFO, WARN, FAIL }
        //private static readonly LEVELS level = LEVELS.INFORM;
        public string LogPath { get; set; }
        public string AppName { get; set; }
        private readonly Stack<string> moduleStack = new Stack<string>();
        public bool Console { get; set; }
        public int Pid { get; set; }
        private bool IncludePidInFileNameFlag { get; set; }
        public DateTime Instance { get; set; }

        public AppLog()
        {
            Instance = DateTime.Now;
            LogPath = System.IO.Path.GetTempPath();
            Pid = Process.GetCurrentProcess().Id;
            AppName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        }

        public void Module(string module)
        {
            Inform("Entering", module);
            moduleStack.Push(module);
        }

        public void Module()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            if (moduleStack.Count >= 1)
            {
                Inform("Exiting", moduleStack.Peek());
                moduleStack.Pop();
            }
            else
            {
                Inform("Attempting to pop an empty moduleStack");
            }
            return;
        }

        public void Enter(string module)
        {
            Inform("Entering", module);
            moduleStack.Push(module);
        }

        public void Leave(string module = "")
        {
            if (moduleStack.Count >= 1)
            {
                Inform("Exiting", moduleStack.Peek());
                moduleStack.Pop();
            }
            else
            {
                Inform("Attempting to pop an empty moduleStack");
            }
            return;
        }

        private string Store(LEVELS lev, string msg)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var stamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffffffzzz");
            var fname = Instance.ToString("yyyy'-'MM'-'dd'-'HH'-'mm") + (IncludePidInFileNameFlag ? $"-{Pid}" : string.Empty) + ".txt";
            string fullMsg = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\r\n",
                stamp,
                lev,
                Pid,
                AppName +
                    (moduleStack.Count > 0 ? (">" + String.Join(">", moduleStack.Reverse().ToArray())) : ""),
                msg);
            string localLogPath = LogPath;
            Directory.CreateDirectory(localLogPath);
            File.AppendAllText(System.IO.Path.Combine(LogPath, fname), fullMsg);
            LastMessage = new R1V(fullMsg);
            if (Console)
            {
                System.Console.Write(fullMsg);
            }
            return fullMsg;
        }

        private string CombineArguments(params object[] args)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var L = new List<string>();
            foreach (object arg in args)
            {
                L.Add($"{arg}");
            }
            return string.Join(" ", L.ToArray());
        }

        public string Debug(params object[] args) => Store(LEVELS.DEBUG, CombineArguments(args));
        public string Inform(params object[] args) => Store(LEVELS.INFO, CombineArguments(args));
        public string Warn(params object[] args) => Store(LEVELS.WARN, CombineArguments(args));
        public string Fail(params object[] args) => Store(LEVELS.FAIL, CombineArguments(args));
        public string Path() => string.Join(">", moduleStack.Reverse());

        public void IncludePidInFileName()
        {
            IncludePidInFileNameFlag = true;
        }
    }

    public class ApplicationLogging
    {
        public static R1V lastMessage;
        private static string _msg;
        private enum LEVELS { INFORMATION, WARNING, ERROR, FATAL }
        private static int _counter;
        private static LEVELS _level = LEVELS.INFORMATION;
        private static string _path = Path.GetTempPath();
        private static string _instance = "";
        private static string _app = "";
        private static readonly Stack<string> _module = new Stack<string>();
        private static bool _gathering;
        private static readonly StringBuilder _gathered = new StringBuilder();
        private static bool _combined;
        private static string _combinedPath = string.Empty;
        private static bool _console;
        private static bool _appfirst;
        private static DateTime _timer = DateTime.Now;
        private static bool _timing;
        private static bool _timestamp;
        private static bool _tabbed;
        private static string tabbedTxt = string.Empty;
        private static bool _includepid;
        public static string _logFile = string.Empty;
        public static string _combinedLogFile = string.Empty;
        public static string _jlogEndPoint = string.Empty;
        public static bool _jlogging;

        public ApplicationLogging()
        {
            _app = "Axtension.";
            _instance = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        }

        public ApplicationLogging(string appName)
        {
            _app = appName;
            _instance = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        }

        public ApplicationLogging(string appName, string reportPath)
        {
            _app = appName;
            _path = reportPath;
            _instance = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        }

        public ApplicationLogging(string appName, string moduleName, string reportPath)
        {
            _app = appName;
            _module.Push(moduleName);
            _path = reportPath;
            _instance = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        }

        public ApplicationLogging(string appName, string moduleName, string reportPath, bool tabbed = false)
        {
            _app = appName;
            _module.Push(moduleName);
            _path = reportPath;
            _instance = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            _tabbed = tabbed;
        }

        public ApplicationLogging JLogging(bool flag, string endpoint)
        {
            _jlogging = flag;
            _jlogEndPoint = endpoint;
            return this;
        }

        public ApplicationLogging SetAppNameTo(string name = "")
        {
            if (name != string.Empty)
            {
                _app = name;
            }
            return this;
        }

        public ApplicationLogging SetReportPathTo(string path = "")
        {
            if (path != string.Empty)
            {
                _path = path;
            }
            return this;
        }

        public ApplicationLogging Combined(bool yesno = false, string path = "", bool tabbed = false)
        {
            _combined = yesno;
            _combinedPath = path;
            _tabbed = tabbed;
            return this;
        }

        public ApplicationLogging ElapsedTime(bool yesno = false)
        {
            _timing = yesno;
            _timer = DateTime.Now;
            return this;
        }

        public ApplicationLogging Timestamp(bool yesno = false)
        {
            _timestamp = yesno;
            return this;
        }

        public ApplicationLogging Gathering(bool gathering)
        {
            _gathering = gathering;
            return this;
        }

        public ApplicationLogging ToConsole(bool flag)
        {
            _console = flag;
            return this;
        }

        public ApplicationLogging IncludePidInFilename(bool flag)
        {
            _includepid = flag;
            return this;
        }

        public ApplicationLogging AlsoToConsole()
        {
            _console = true;
            return this;
        }

        public ApplicationLogging WithAppFirst()
        {
            _appfirst = true;
            return this;
        }

        public ApplicationLogging Module(string module)
        {
            _module.Push(module);
            return this;
        }

        public ApplicationLogging Module()
        {
            if (_module.Count > 1) // never let the stack be empty
            {
                _module.Pop();
            }
            else
            {
                WriteMessageAndLevelToFile("WARNING: Tried to empty the Module stack.", LEVELS.WARNING);
            }
            return this;
        }

        public ApplicationLogging Informational()
        {
            _level = LEVELS.INFORMATION;
            return this;
        }

        public ApplicationLogging Warning()
        {
            _level = LEVELS.WARNING;
            return this;
        }

        public ApplicationLogging Error()
        {
            _level = LEVELS.ERROR;
            return this;
        }

        public ApplicationLogging Fatal()
        {
            _level = LEVELS.FATAL;
            return this;
        }

        public ApplicationLogging Category(string cat)
        {
            _level = cat.Substring(0, 1).ToUpper() switch
            {
                "I" => LEVELS.INFORMATION,
                "W" => LEVELS.WARNING,
                "F" => LEVELS.FATAL,
                "E" => LEVELS.ERROR,
                _ => LEVELS.INFORMATION,
            };
            return this;
        }

        private void WriteMessageAndLevelToFile(string message, LEVELS level)
        {
            WriteMessageAndLevelToFile(message, level.ToString().ToUpper());
        }

        private void WriteMessageAndLevelToFile(string message, string level)
        {
            message = message.Replace("\r\n", "\r").Replace("\n", "\r").Replace("\t", "    ");
            string[] stack = _module.ToArray();
            Array.Reverse(stack);
            string stackPath = string.Join(".", stack);
            string txt;

            if (_jlogging) // endpoint?p=exe & m=module & t = timestamp & l = level 
                           // text in post body
            {
                RESTful R = new RESTful(_jlogEndPoint)
                    .Verb("POST")
                    .Tail($"s={Environment.GetEnvironmentVariable("COMPUTERNAME")}")
                    .Tail($"p={System.AppDomain.CurrentDomain.FriendlyName}")
                    .Tail($"m={stackPath}")
                    .Tail($"d={DateTime.Now:HH:mm:ss.ffff}")
                    .Tail($"l={level.Substring(0, 1).ToUpper()}")
                    .Body(message)
                    .Send();
            }

            if (_timing)
            {
                TimeSpan ts = DateTime.Now - _timer;
                txt = string.Format("({0})-{1}-[{2:D6}]: {3}", level.Substring(0, 1).ToUpper(), stackPath, ts.Milliseconds, message);
                tabbedTxt = string.Format("{0}\t{1}\t{2:D6}\t{3}", level.Substring(0, 1).ToUpper(), stackPath, ts.Milliseconds, message);
                _timer = DateTime.Now;
                _counter++;
            }
            else
            {
                if (_timestamp)
                {
                    txt = string.Format("({0})-{1}-[{2}]: {3}", level.Substring(0, 1).ToUpper(), stackPath, DateTime.Now.ToString("HH:mm:ss.ffff"), message);
                    tabbedTxt = string.Format("{0}\t{1}\t{2}\t{3}", level.Substring(0, 1).ToUpper(), stackPath, DateTime.Now.ToString("HH:mm:ss.ffff"), message);
                }
                else
                {
                    txt = string.Format("({0})-{1}-[{2}]: {3}", level.Substring(0, 1).ToUpper(), stackPath, _counter++, message);
                    tabbedTxt = string.Format("{0}\t{1}\t{2}\t{3}", level.Substring(0, 1).ToUpper(), stackPath, _counter++, message);
                }
            }

            if (_gathering)
            {
                _gathered.Append(txt).Append("\r\n");
            }

            int retry = 0;
            var appPid = _app + (_includepid ? $"[{Process.GetCurrentProcess().Id}]-" : string.Empty);
            _logFile = Path.Combine(_path,
                (_appfirst ? appPid + _instance : _instance + "-" + appPid) +
                ".txt");

            while (true)
            {
                try
                {
                    if (_tabbed)
                    {
                        System.IO.File.AppendAllText(_logFile, tabbedTxt + "\r\n");
                    }
                    else
                    {
                        System.IO.File.AppendAllText(_logFile, txt + "\r\n");
                    }
                    break;
                }
                catch (DirectoryNotFoundException)
                {
                    System.IO.Directory.CreateDirectory(_path);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(10);
                    retry++;
                    if (retry > 3)
                    {
                        break; //forget about it
                    }
                }
            }

            if (_console)
            {
                Console.WriteLine(txt);
            }

            if (_combined)
            {
                _combinedLogFile = System.IO.Path.Combine(_combinedPath, _instance + "-Combined.txt");
                retry = 0;
                while (true)
                {
                    try
                    {
                        if (_tabbed)
                        {
                            System.IO.File.AppendAllText(_combinedLogFile, tabbedTxt + "\r\n");
                        }
                        else
                        {
                            System.IO.File.AppendAllText(_combinedLogFile, txt + "\r\n");
                        }
                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        System.IO.Directory.CreateDirectory(_combinedPath);
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(10);
                        retry++;
                        if (retry > 3)
                        {
                            break; //forget about it
                        }
                    }
                }
            }
        }

        public ApplicationLogging Send(params object[] args)
        {
            System.Collections.Generic.List<string> L = new System.Collections.Generic.List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                string sArg;
                if (args[i] == null)
                {
                    sArg = "(null)\r\n";
                    if (i == 0)
                    {
                        System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                        var sb = new StringBuilder();
                        foreach (var frame in t.GetFrames())
                        {
                            sb.Append(frame.ToString());
                        }
                        sArg += sb.ToString();
                    }
                }
                else
                {
                    sArg = args[i].ToString();
                }
                L.Add(sArg);
            }
            _msg = String.Join(" ", L.ToArray());
            WriteMessageAndLevelToFile(_msg, _level);
            lastMessage = new R1V(_msg);
            return this;
        }

        public ApplicationLogging Inform(params object[] args) => Informational().Send(args);

        public ApplicationLogging Inform(string msg) => Informational().Send(msg);
        //{
        //    WriteMessageAndLevelToFile(msg, LEVELS.INFORMATION);
        //    return this;
        //}

        public ApplicationLogging Warn(params object[] args) => Warning().Send(args);

        public ApplicationLogging Warn(string msg) => Warning().Send(msg);
        //{
        //    WriteMessageAndLevelToFile(msg, LEVELS.WARNING);
        //    return this;
        //}

        public ApplicationLogging Fail(params object[] args) => Error().Send(args);

        public ApplicationLogging Fail(string msg) => Error().Send(msg);
        //{
        //    WriteMessageAndLevelToFile(msg, LEVELS.FATAL);
        //    return this;
        //}

        public string GetGathered()
        {
            return _gathered.ToString();
        }
    }
}
