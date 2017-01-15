using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static TaskbarTool.Constants;
using TaskbarTool.Structs;
using TaskbarTool.Enums;

namespace TaskbarTool
{
    public static class Taskbars
    {
        public static List<Taskbar> Bars { get; set; }
        public static bool MaximizedStateChanged { get; set; }
        private static string tbType;

        static Taskbars()
        {
            Bars = new List<Taskbar>();
            MaximizedStateChanged = true;
        }
        
        public static void ApplyStyles(Taskbar taskbar)
        {
            var sizeOfPolicy = Marshal.SizeOf(taskbar.AccentPolicy);
            var policyPtr = Marshal.AllocHGlobal(sizeOfPolicy);
            Marshal.StructureToPtr(taskbar.AccentPolicy, policyPtr, false);

            var data = new WinCompatTrData(WindowCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy);

            Externals.SetWindowCompositionAttribute(taskbar.HWND, ref data);

            Marshal.FreeHGlobal(policyPtr);
        }

        public static void UpdateMaximizedState()
        {
            foreach (var tb in Bars)
                tb.FindMaximizedWindowsHere();
            MaximizedStateChanged = false;
        }

        public static void UpdateAllSettings()
        {
            foreach (var tb in Bars)
            {
                if (tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
                tb.AccentPolicy.GradientColor = Globals.GetTaskbarColor(tbType);
            }
        }

        public static void UpdateAccentState()
        {
            foreach (var tb in Bars)
            {
                tbType = tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized
                    ? "Maximized"
                    : "Main";
                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
            }
        }

        public static void UpdateAccentFlags()
        {
            foreach (var tb in Bars)
            {
                tbType = tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized
                    ? "Maximized"
                    : "Main";

                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
            }
        }

        public static void UpdateColor()
        {
            foreach (var tb in Bars)
            {
                tbType = tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized
                    ? "Maximized"
                    : "Main";

                tb.AccentPolicy.GradientColor = Globals.GetTaskbarColor(tbType);
            }
        }
    }

    public class Taskbar
    {
        public IntPtr HWND { get; set; }
        public IntPtr Monitor { get; set; }
        public bool HasMaximizedWindow { get; set; }
        public AccentPolicy AccentPolicy;

        public Taskbar(IntPtr hwnd)
        {
            HWND = hwnd;
            Monitor = Externals.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            AccentPolicy = new AccentPolicy();


            FindMaximizedWindowsHere();
        }

        public void FindMaximizedWindowsHere()
        {
            var isInThisScreen = false;
            IntPtr thisAppMonitor;

            foreach (var hwnd in Globals.MaximizedWindows)
            {
                thisAppMonitor = Externals.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                if (Monitor == thisAppMonitor) { isInThisScreen = true; }
            }

            HasMaximizedWindow = isInThisScreen;
        }
    }
}
