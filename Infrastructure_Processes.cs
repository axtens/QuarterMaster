using QuarterMaster.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;

namespace QuarterMaster.Infrastructure
{
    public static class Processes
    {
        internal static ApplicationLogging AL;

        public static List<PropertyDataCollection> ChildProcessesOfParentProcess(uint pid)
        {
            AL?.Module("ChildProcessesOfParentProcess");

            string WQL = string.Format("SELECT * FROM Win32_Process WHERE ParentProcessId = {0}", pid);

            List<PropertyDataCollection> mol = new List<PropertyDataCollection>();

            ManagementObjectSearcher mgmtObjSearcher = new ManagementObjectSearcher(WQL);
            ManagementObjectCollection objCol = mgmtObjSearcher.Get();

            AL?.Inform(objCol.Count, "items found using", WQL);

            if (objCol.Count != 0)
            {
                mol.AddRange(from ManagementObject Process in objCol.Cast<ManagementObject>()
                             select Process.Properties);
            }
            AL?.Module();
            return mol;
        }

        public static List<PropertyDataCollection> ChildProcessesOfCurrentProcess() =>
            ChildProcessesOfParentProcess((uint)Process.GetCurrentProcess().Id);

        public static List<PropertyDataCollection> ChildProcessesOfProcess(uint pid)
        {
            AL?.Module("ChildProcessesOfProcess");

            string WQL = string.Format("SELECT * FROM Win32_Process WHERE ProcessId = {0}", pid);
            List<PropertyDataCollection> mol = new List<PropertyDataCollection>();

            ManagementObjectSearcher mgmtObjSearcher = new ManagementObjectSearcher(WQL);
            ManagementObjectCollection objCol = mgmtObjSearcher.Get();
            AL?.Inform(objCol.Count, "items found using", WQL);
            if (objCol.Count != 0)
            {
                mol.AddRange(from ManagementObject Process in objCol.Cast<ManagementObject>()
                             select Process.Properties);
            }

            AL?.Module();
            return mol;
        }

        public static List<PropertyDataCollection> ChildProcessesOfProcess() =>
            ChildProcessesOfProcess((uint)Process.GetCurrentProcess().Id);

        public static uint ParentProcessOfProcess(uint pid)
        {
            AL?.Module("ParentProcessOfProcess");

            uint result = UInt32.MinValue;
            string WQL = string.Format("SELECT * FROM Win32_Process WHERE ProcessId = {0}", pid);
            ManagementObjectSearcher mgmtObjSearcher = new ManagementObjectSearcher(WQL);
            ManagementObjectCollection objCol = mgmtObjSearcher.Get();
            AL?.Inform(objCol.Count, "items found using", WQL);
            if (objCol.Count != 0)
            {
                foreach (ManagementObject Process in objCol.Cast<ManagementObject>())
                {
                    result = (uint)Process.Properties["ParentProcessId"].Value;
                }
            }

            AL?.Module();

            return result;
        }

        public static uint ParentProcessOfCurrentProcess() =>
            ParentProcessOfProcess((uint)Process.GetCurrentProcess().Id);

        // specific uses:
        //  with every chromedriver. 
        //      get parent.
        //      if parent is an instance of NormanEmailScraper
        //          get children of that instance of NES and see if it has a pid matching the pid of chromedriver.
        //          if pid is not matching, then 
        //              terminate the chromedriver and all its subprocesses.
        //      else
        //          terminte chromedriver process and subprocesses

        public static List<string> GetCommandLines(string processName)
        {
            AL?.Module("GetCommandLines");
            List<string> answer = new List<string>();
            Process[] procs = Process.GetProcessesByName(processName);
            foreach (Process proc in procs)
            {
                List<PropertyDataCollection> lPDC = ChildProcessesOfProcess((uint)proc.Id);
                foreach (PropertyDataCollection pdc in lPDC)
                {
                    answer.Add(pdc["CommandLine"].Value.ToString());
                }
            }
            AL?.Module();
            return answer;
        }

        public static bool TerminateUnownedChromeDriverInstances()
        {
            AL?.Module("TerminateUnownedChromeDriverInstances");

            bool status = false;
            Process[] chromedrivers = Process.GetProcessesByName("chromedriver");
            Process parentProcess;
            foreach (Process chromedriver in chromedrivers)
            {
                uint parentId = ParentProcessOfProcess((uint)chromedriver.Id);
                AL?.Inform("Want to kill", chromedriver.Id, "and subprocesses.");
                try
                {
                    parentProcess = Process.GetProcessById((int)parentId);
                }
                catch (Exception E)
                {
                    if (AL != null)
                    {
                        AL.Warn("Did not find parent", parentId, "of", chromedriver.Id);
                        AL.Warn(E.Message);
                    }
                    TerminateBottomUp((uint)chromedriver.Id);
                    status = true;
                    continue;
                }

                //if (parentProcess.ProcessName.ToLower() == "normanemailscraper")
                //{
                AL?.Inform(parentProcess.ProcessName, "found");
                List<PropertyDataCollection> cpList = ChildProcessesOfParentProcess((uint)parentId);
                bool chromeDriverIdsMatch = false;
                foreach (PropertyDataCollection pdc in cpList)
                {
                    AL?.Inform("Testing", pdc["ProcessId"].Value, chromedriver.Id);

                    if ((uint)pdc["ProcessId"].Value == (uint)chromedriver.Id)
                    {
                        chromeDriverIdsMatch = true;
                        AL?.Inform("Matched!");
                    }
                }
                if (!chromeDriverIdsMatch)
                {
                    AL?.Warn(parentProcess.ProcessName, "was not the owner of the chromedriver");
                    TerminateBottomUp((uint)chromedriver.Id);
                    status = true;
                }
                //}
                //else
                //{
                //    AL?.Warn(parentProcess.ProcessName, "was not an instance of NormanEmailScraper.");
                //    TerminateBottomUp((uint)chrome.Id);
                //    status = true;
                //}
            }

            AL?.Module();

            return status;
        }

        private static void StackChildren(ref Stack<uint> pidStack, uint id)
        {
            AL?.Module("StackChildren");
            List<PropertyDataCollection> children = ChildProcessesOfParentProcess(id);
            foreach (PropertyDataCollection child in children)
            {
                uint childId = (uint)child["ProcessId"].Value;
                pidStack.Push(childId);
                StackChildren(ref pidStack, childId);
            }
            AL?.Module();
        }

        private static void TerminateBottomUp(uint id)
        {
            AL?.Module("TerminateBottomUp");
            Stack<uint> pidStack = new Stack<uint>();
            pidStack.Push(id);
            StackChildren(ref pidStack, id);
            while (pidStack.Count > 0)
            {
                uint pid = pidStack.Pop();
                AL?.Inform("Killing", pid);
                Process p = Process.GetProcessById((int)pid);
                try
                {
                    p.Kill();
                }
                catch (Exception E)
                {
                    AL?.Fail("Could not kill", pid, E.Message);
                }
            }
        }

        public static void RegisterApplicationLogging(ref Logging.ApplicationLogging ptr)
        {
            AL = ptr;
        }

        public static string GetCommandLine(Process proc)
        {
            AL?.Module("getCommandLine");
            ManagementObjectSearcher commandLineSearcher = new ManagementObjectSearcher(
                "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + proc.Id);
            String commandLine = "";
            foreach (ManagementObject commandLineObject in commandLineSearcher.Get().Cast<ManagementObject>())
            {
                commandLine += (String)commandLineObject["CommandLine"];
            }
            AL?.Inform(commandLine);
            AL?.Module();
            return commandLine;
        }

        public static string WhatsRunning()
        {
            StringBuilder answer = new StringBuilder();
            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                string procCmd = GetCommandLine(proc);
                if (procCmd.Length == 0)
                {
                    procCmd = proc.ProcessName;
                }
                answer.Append(procCmd).Append("\r\n");
            }
            return answer.ToString();
        }

        public static string WhatsRunning(string procname)
        {
            StringBuilder answer = new StringBuilder();
            Process[] procs = Process.GetProcessesByName(procname);
            foreach (Process proc in procs)
            {
                string procCmd = GetCommandLine(proc);
                answer.Append(procCmd).Append("\r\n");
            }
            return answer.ToString();
        }

        public static void ExecuteCommand(string command)
        {
            AL?.Module("ExecuteCommand");
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var process = Process.Start(processInfo);
            System.Threading.Thread.Sleep(10000);
            AL?.Inform(processInfo.Arguments);
            process.Close();
            AL?.Module();
        }

        public static bool RestartService(string serviceName, int timeoutMilliseconds)
        {
            AL?.Module("RestartService");
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                AL?.Module();
                return true;
            }
            catch (Exception exc)
            {
                // ...
                AL?.Warn("RestartService", exc.Message, exc.Source, exc.StackTrace);
                AL?.Module();
                return false;
            }
        }

        public static string WatchRunOf(string processName, DateTime datetime, int id)
        {
            string wql = string.Format(@"SELECT * 
                    FROM Win32_Process 
                    WHERE ProcessId = {0} AND StartTime = #{1}# AND ProcessName = '{2}'", id, datetime, processName);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            String answer = "";
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                answer = obj.ToString();
            }
            return answer;
        }

        public static int KillAll(string procName)
        {
            Process[] procs = Process.GetProcessesByName(procName);
            var cnt = 0;
            foreach (Process proc in procs)
            {
                TerminateBottomUp((uint)proc.Id);
                cnt++;
            }
            return cnt;
        }

        public static bool ProcessExists(int id)
        {
            try
            {
                Process process = Process.GetProcessById(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IntPtr ProcessHwnd(int id)
        {
            try
            {
                Process process = Process.GetProcessById(id);
                return process.MainWindowHandle;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public static string DictDump(IDictionary data)
        {
            var sb = new StringBuilder();
            foreach (KeyValuePair<object, object> kvp in data)
            {
                sb.Append(kvp.Key).Append(" => ").Append(kvp.Value).AppendLine();
            }
            return sb.ToString();
        }

        public static List<Process> ListAllProcessesStartedBy(UInt32 parentProcessId)
        {
            List<Process> procList = new List<Process>();
            InternalListAllProcessesStartedBy(ref procList, parentProcessId);
            return procList;
        }

        private static void InternalListAllProcessesStartedBy(ref List<Process> procList, UInt32 parentProcessId)
        {
            // NOTE: Process Ids are reused!
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT * " +
                "FROM Win32_Process " +
                "WHERE ParentProcessId=" + parentProcessId);
            ManagementObjectCollection collection = searcher.Get();
            if (collection.Count > 0)
            {
                foreach (var item in collection)
                {
                    UInt32 childProcessId = (UInt32)item["ProcessId"];
                    if ((int)childProcessId != Process.GetCurrentProcess().Id)
                    {
                        InternalListAllProcessesStartedBy(ref procList, childProcessId);
                        try
                        {
                            Process childProcess = Process.GetProcessById((int)childProcessId);
                            procList.Add(childProcess);
                        }
                        catch
                        {
                            AL?.Warning().Send($"Process with ID of {childProcessId} not found.");
                        }
                    }
                }
            }
        }

        public static void CrashHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            AL?.Error().Send($"CrashHandler. {Processes.MachineDayHourProcess()}");
            AL?.Error().Send("Message: unhandled exception caught: " + e.Message);
            AL?.Error().Send("StackTrace:", e.StackTrace);
            AL?.Error().Send("Data:", DictDump(e.Data));
            AL?.Error().Send($"Runtime terminating: {args.IsTerminating}");
        }

        public static string MachineDayHourProcess()
        {
            var dayHour = DateTime.Now.ToString("dd'-'HH");
            var machine = Environment.GetEnvironmentVariable("USERDOMAIN").Split('-')[1];
            var process = Process.GetCurrentProcess().ProcessName;
            return $"{machine}:{dayHour} {process}";
        }
    }
}
