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
            int sizeOfPolicy = Marshal.SizeOf(taskbar.AccentPolicy);
            IntPtr policyPtr = Marshal.AllocHGlobal(sizeOfPolicy);
            Marshal.StructureToPtr(taskbar.AccentPolicy, policyPtr, false);

            WinCompatTrData data = new WinCompatTrData(WindowCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy);

            Externals.SetWindowCompositionAttribute(taskbar.HWND, ref data);

            Marshal.FreeHGlobal(policyPtr);
        }

        public static void UpdateMaximizedState()
        {
            foreach (Taskbar tb in Bars)
            {
                tb.FindMaximizedWindowsHere();
            }
            MaximizedStateChanged = false;
        }

        public static void UpdateAllSettings()
        {
            foreach (Taskbar tb in Bars)
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
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentState = Globals.GetAccentState(tbType);
            }
        }

        public static void UpdateAccentFlags()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

                tb.AccentPolicy.AccentFlags = Globals.GetAccentFlags(tbType);
            }
        }

        public static void UpdateColor()
        {
            foreach (Taskbar tb in Bars)
            {
                if (tb.HasMaximizedWindow && TT.Options.Settings.UseDifferentSettingsWhenMaximized) { tbType = "Maximized"; }
                else { tbType = "Main"; }

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
            bool isInThisScreen = false;
            IntPtr thisAppMonitor;

            foreach (IntPtr hwnd in Globals.MaximizedWindows)
            {
                thisAppMonitor = Externals.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                if (Monitor == thisAppMonitor) { isInThisScreen = true; }
            }

            HasMaximizedWindow = isInThisScreen;
            return;
        }
    }
}
