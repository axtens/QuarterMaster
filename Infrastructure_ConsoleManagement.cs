﻿using System;
using System.Runtime.InteropServices;

namespace QuarterMaster.Infrastructure
{
    public static class ConsoleManagement
    {
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static void DeleteCloseButton()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
        }
    }
}
