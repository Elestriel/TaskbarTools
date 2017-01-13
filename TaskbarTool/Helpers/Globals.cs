using System;
using System.Collections.Generic;
using System.Windows.Media;
using TaskbarTool.Enums;
using TaskbarTool.Structs;

namespace TaskbarTool
{
    public static class Globals
    {
        public static List<IntPtr> MaximizedWindows = new List<IntPtr>();
        public static List<Taskbar> HwndMonitors = new List<Taskbar>();
        public static string TaskbarBeingEdited;
        public static int WindowsAccentColor;
        //public static SettingsClass TaskbarSettings = new SettingsClass();

        private static Int32 ColorToInt32(string color)
        {
            Color thisColor = (Color)ColorConverter.ConvertFromString(color);
            return ColorToInt32(thisColor);
        }

        private static Int32 ColorToInt32(Color color)
        {
            return (Int32)BitConverter.ToInt32(new byte[] { color.R, color.G, color.B, color.A }, 0);
        }

        #region TaskbarColor
        public static Int32 GetTaskbarColor(string taskbar)
        {
            if (taskbar == "Main")
            {
                if (TT.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor)
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], TT.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else { return ColorToInt32(TT.Options.Settings.MainTaskbarStyle.GradientColor); }
            }
            else
            {
                if (TT.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor)
                {
                    byte[] bytes = BitConverter.GetBytes(WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], TT.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else { return ColorToInt32(TT.Options.Settings.MaximizedTaskbarStyle.GradientColor); }
            }
        }

        public static void SetTaskbarColor(Color color)
        {
            SetTaskbarColor(TaskbarBeingEdited, color);
        }

        public static void SetTaskbarColor(string taskbar, Color color)
        {
            if (taskbar == "Main")
            {
                TT.Options.Settings.MainTaskbarStyle.GradientColor = color.ToString();
            }
            else
            {
                TT.Options.Settings.MaximizedTaskbarStyle.GradientColor = color.ToString();
            }
        }
        #endregion TaskbarColor

        #region AccentFlags
        public static int GetAccentFlags(string taskbar)
        {
            if (taskbar == "Main")
            {
                if (TT.Options.Settings.MainTaskbarStyle.Colorize) { return 2; }
                else { return 0; }
            }
            else
            {
                if (TT.Options.Settings.MaximizedTaskbarStyle.Colorize) { return 2; }
                else { return 0; }
            }
        }

        public static void SetAccentFlags(bool colorize)
        {
            SetAccentFlags(TaskbarBeingEdited, colorize);
        }

        public static void SetAccentFlags(string taskbar, bool colorize)
        {
            if (taskbar == "Main")
            {
                TT.Options.Settings.MainTaskbarStyle.Colorize = colorize;
            }
            else
            {
                TT.Options.Settings.MaximizedTaskbarStyle.Colorize = colorize;
            }
        }
        #endregion AccentFlags

        #region AccentState
        public static AccentState GetAccentState(string taskbar)
        {
            if (taskbar == "Main")
            {
                return (AccentState)TT.Options.Settings.MainTaskbarStyle.AccentState;
            }
            else
            {
                return (AccentState)TT.Options.Settings.MaximizedTaskbarStyle.AccentState;
            }
        }

        public static void SetAccentState(AccentState state)
        {
            SetAccentState(TaskbarBeingEdited, state);
        }

        public static void SetAccentState(string taskbar, AccentState state)
        {
            if (taskbar == "Main")
            {
                TT.Options.Settings.MainTaskbarStyle.AccentState = (byte)state;
            }
            else
            {
                TT.Options.Settings.MaximizedTaskbarStyle.AccentState = (byte)state;
            }
        }
        #endregion AccentState

        #region UseAccentColor
        public static void SetUseAccentColor(bool use)
        {
            SetUseAccentColor(TaskbarBeingEdited, use);
        }

        public static void SetUseAccentColor(string taskbar, bool use)
        {
            if (taskbar == "Main")
            {
                TT.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor = use;
            }
            else
            {
                TT.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor = use;
            }
        }
        #endregion UseAccentColor

        #region WindowsAccentAlpha
        public static void SetWindowsAccentAlpha(byte alpha)
        {
            SetWindowsAccentAlpha(TaskbarBeingEdited, alpha);
        }

        public static void SetWindowsAccentAlpha(string taskbar, byte alpha)
        {
            if (taskbar == "Main")
            {
                TT.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha = alpha;
            }
            else
            {
                TT.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha = alpha;
            }
        }
        #endregion WindowsAccentAlpha
    }

    public class SettingsClass
    {
        private AccentState _AccentState;
        private int _AccentFlags;
        private int _GradientColor;
        private int _WindowsAccentColor;
        private bool _UseWindowsAccentColor;
        private byte _WindowsAccentAlpha;

        public AccentState AccentState
        {
            get
            {
                return _AccentState;
            }
            set
            {
                _AccentState = value;
                Taskbars.UpdateAccentState();
            }
        }

        public int AccentFlags
        {
            get
            {
                return _AccentFlags;
            }
            set
            {
                _AccentFlags = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        public int GradientColor
        {
            get
            {
                return _GradientColor;
            }
            set
            {
                _GradientColor = value;
                Taskbars.UpdateColor();
            }
        }

        public int WindowsAccentColor
        {
            get
            {
                return _WindowsAccentColor;
            }
            set
            {
                _WindowsAccentColor = value;
                Taskbars.UpdateColor();
            }
        }

        public bool UseWindowsAccentColor
        {
            get
            {
                return _UseWindowsAccentColor;
            }
            set
            {
                _UseWindowsAccentColor = value;
                Taskbars.UpdateColor();
            }
        }

        public byte WindowsAccentAlpha
        {
            get
            {
                return _WindowsAccentAlpha;
            }
            set
            {
                _WindowsAccentAlpha = value;
                if (_UseWindowsAccentColor) { Taskbars.UpdateColor(); }
            }
        }

        public int Color
        {
            get
            {
                if (_UseWindowsAccentColor)
                {
                    byte[] bytes = BitConverter.GetBytes(_WindowsAccentColor);
                    int colorInt = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], bytes[2], _WindowsAccentAlpha }, 0);
                    return colorInt;
                }
                else { return _GradientColor; }
            }
        }
    }
}
