using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuarterMaster.Infrastructure
{
    public static class Resources
    {
        [DllImport("user32.dll")]
        static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);

        public static uint GetGuiResourcesGDICount(IntPtr procPtr)
        {
            return GetGuiResources(procPtr, 0); // Process.GetCurrentProcess().Handle
        }

        public static uint GetGuiResourcesUserCount(IntPtr procPtr)
        {
            return GetGuiResources(procPtr, 1);
        }

        public static uint GetGuiResourcesGDICount(Process proc)
        {
            return GetGuiResources(proc.Handle, 0); // Process.GetCurrentProcess().Handle
        }

        public static uint GetGuiResourcesUserCount(Process proc)
        {
            return GetGuiResources(proc.Handle, 1);
        }
    }
}
