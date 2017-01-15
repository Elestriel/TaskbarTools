using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

using static TaskbarTool.Constants;
using static TaskbarTool.Globals;
using TaskbarTool.Enums;
using TaskbarTool.Structs;
using Microsoft.Win32;
using System.Reflection;

namespace TaskbarTool {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region Declarations
        // Main window initialization
        bool WindowInitialized = false;
        readonly string MyPath = Assembly.GetExecutingAssembly().Location;

        // Taskbars
        static Task ApplyTask;
        static bool RunApplyTask = false;
        public static bool FindTaskbarHandles = true;

        static System.Windows.Forms.NotifyIcon SysTrayIcon;
        readonly ContextMenu SysTrayContextMenu;

        private static bool alphaDragStarted = false;

        // Explorer restarts and Windows Accent Colour changes
        private static readonly uint WM_TASKBARCREATED = Externals.RegisterWindowMessage("TaskbarCreated");

        // Window state hook
        private static readonly Externals.WinEventDelegate procDelegate = new Externals.WinEventDelegate(WinEventProc);
        private static IntPtr WindowStateHook;
        private static IntPtr LastClosedWindow;
        private static DateTime LastClosedWindowTime;

        // Start with Windows registry key
        readonly RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        #endregion Declarations

        #region Initializations
        public MainWindow() {
            InitializeComponent ();

            SysTrayContextMenu = this.FindResource ("TrayContextMenu") as ContextMenu;

            var iconStream = Application.GetResourceStream (new Uri ("Resources/tt-logo.ico", UriKind.Relative)).Stream;
            SysTrayIcon = new System.Windows.Forms.NotifyIcon {
                Icon = new System.Drawing.Icon (iconStream),
                Visible = true
            };
            SysTrayIcon.MouseClick += SysTrayIcon_MouseClick;
            SysTrayIcon.DoubleClick += (sender, args) => {
                this.Show ();
                this.WindowState = WindowState.Normal;
            };
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized)
                this.Hide ();

            base.OnStateChanged (e);
        }

        private void PopulateComboBoxes() {
            AccentStateComboBox.ItemsSource = Enum.GetValues (typeof (AccentState)).Cast<AccentState> ();
            AccentStateComboBox.SelectedIndex = 0;
        }

        private void LoadSettings() {
            TT.InitializeOptions ();

            SwitchTaskbarBeingEdited ("Main");

            UseMaximizedSettingsCheckBox.IsChecked = TT.Options.Settings.UseDifferentSettingsWhenMaximized;
            StartMinimizedCheckBox.IsChecked = TT.Options.Settings.StartMinimized;
            StartWhenLaunchedCheckBox.IsChecked = TT.Options.Settings.StartWhenLaunched;
        }

        private void SaveSettings() {
            TT.Options.Settings.MainTaskbarStyle.AccentState = (byte) ((int) AccentStateComboBox.SelectedItem);
            TT.Options.Settings.MainTaskbarStyle.GradientColor = GradientColorPicker.SelectedColor.ToString ();
            TT.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha = (byte) WindowsAccentAlphaSlider.Value;
            TT.Options.Settings.MainTaskbarStyle.Colorize = ColorizeBlurCheckBox.IsChecked ?? false;
            TT.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor = WindowsAccentColorCheckBox.IsChecked ?? false;

            TT.Options.Settings.UseDifferentSettingsWhenMaximized = UseMaximizedSettingsCheckBox.IsChecked ?? false;
            TT.Options.Settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            TT.Options.Settings.StartWhenLaunched = StartWhenLaunchedCheckBox.IsChecked ?? false;

            TT.SaveOptions ();
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            PopulateComboBoxes ();
            LoadSettings ();
            WindowInitialized = true;

            if (TT.Options.Settings.StartMinimized)
                WindowState = WindowState.Minimized;
            if (TT.Options.Settings.StartWhenLaunched)
                StartStopButton_Click (null, null);

            // Listen for name change changes across all processes/threads on current desktop
            WindowStateHook = Externals.SetWinEventHook (EVENT_MIN, EVENT_MAX, IntPtr.Zero, procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }
        #endregion Initializations

        #region Destructors
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            SysTrayIcon.Dispose ();
            SaveSettings ();
            RunApplyTask = false;
            Externals.UnhookWinEvent (WindowStateHook);
        }

        private void CloseMainWindow(object sender, RoutedEventArgs e) {
            this.Close ();
        }
        #endregion Destructors

        #region Functions
        private void ApplyToAllTaskbars() {
            Taskbars.Bars = new List<Taskbar> ();

            while (RunApplyTask) {
                if (FindTaskbarHandles) {
                    Taskbars.Bars.Add (new Taskbar (Externals.FindWindow ("Shell_TrayWnd", null)));
                    //TaskbarList.Add(FindWindow(null, "Pins view"));
                    var otherBars = IntPtr.Zero;

                    //IntPtr cortana = FindWindowEx(hWndList[0], IntPtr.Zero, "TrayDummySearchControl", null);
                    //hWndList.Add(cortana);

                    while (true) {
                        otherBars = Externals.FindWindowEx (IntPtr.Zero, otherBars, "Shell_SecondaryTrayWnd", "");
                        if (otherBars == IntPtr.Zero) { break; }
                        Taskbars.Bars.Add (new Taskbar (otherBars));
                    }

                    FindTaskbarHandles = false;

                    App.Current.Dispatcher.Invoke (UpdateAllSettings);
                }

                if (Taskbars.MaximizedStateChanged) {
                    Taskbars.UpdateMaximizedState ();
                    Taskbars.UpdateAllSettings ();
                }

                foreach (var taskbar in Taskbars.Bars) {
                    Taskbars.ApplyStyles (taskbar);
                }

                Thread.Sleep (10);
            }
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized (e);

            var mainWindowPtr = new WindowInteropHelper (this).Handle;
            var mainWindowSrc = HwndSource.FromHwnd (mainWindowPtr);
            mainWindowSrc.AddHook (WndProc);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == WM_TASKBARCREATED) {
                FindTaskbarHandles = true;
                handled = true;
            }
            else if (msg == WM_DWMCOLORIZATIONCOLORCHANGED) {
                Globals.WindowsAccentColor = WindowsAccentColor.GetColorAsInt (); // TODO: use colour from wParam
                handled = true;
            }

            return IntPtr.Zero;
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            //if ((idObject != 0 || idChild != 0) { return; }

            var placement = new WindowPlacement ();
            placement.length = Marshal.SizeOf (placement);
            Externals.GetWindowPlacement (hwnd, ref placement);

            // Window is closing
            if (idObject == -2 && idChild == 5) {
                if (!MaximizedWindows.Contains (hwnd))
                    return;
                LastClosedWindow = hwnd;
                LastClosedWindowTime = DateTime.Now;
                MaximizedWindows.Remove (hwnd);
                Taskbars.MaximizedStateChanged = true;
            }
            else if (placement.showCmd == WindowPlacementCommands.SW_MAXIMIZE || placement.showCmd == WindowPlacementCommands.SW_SHOWMAXIMIZED) {
                if (MaximizedWindows.Contains (hwnd)
                    || LastClosedWindow == hwnd && (DateTime.Now - LastClosedWindowTime).TotalSeconds < 1)
                    return;
                MaximizedWindows.Add (hwnd);
                Taskbars.MaximizedStateChanged = true;
            }
            else if (placement.showCmd == WindowPlacementCommands.SW_NORMAL) {
                if (!MaximizedWindows.Contains (hwnd))
                    return;
                MaximizedWindows.Remove (hwnd);
                Taskbars.MaximizedStateChanged = true;
            }
            else if (placement.showCmd == WindowPlacementCommands.SW_SHOWMINIMIZED || placement.showCmd == WindowPlacementCommands.SW_MINIMIZE) {
                if (!MaximizedWindows.Contains (hwnd))
                    return;
                MaximizedWindows.Remove (hwnd);
                Taskbars.MaximizedStateChanged = true;
            }
        }

        private void UpdateAllSettings() {
            SetAccentState ((AccentState) AccentStateComboBox.SelectedItem);
            SetTaskbarColor (GradientColorPicker.SelectedColor ?? Colors.White);
            SetAccentFlags (ColorizeBlurCheckBox.IsChecked ?? false);
            WindowsAccentColorCheckBox_Changed (null, null);
            SetWindowsAccentAlpha ((byte) WindowsAccentAlphaSlider.Value);

            Taskbars.UpdateAllSettings ();
        }

        private void SwitchTaskbarBeingEdited(string switchTo) {
            TaskbarBeingEdited = switchTo;
            ShowTaskbarSettings (TaskbarBeingEdited);
            EditSwitchButton.Content = $"{TaskbarBeingEdited} Taskbar";
        }

        private void ShowTaskbarSettings(string tb) {
            if (tb == "Main") {
                AccentStateComboBox.SelectedItem = (AccentState) TT.Options.Settings.MainTaskbarStyle.AccentState;
                GradientColorPicker.SelectedColor = (Color) ColorConverter.ConvertFromString (TT.Options.Settings.MainTaskbarStyle.GradientColor);
                WindowsAccentAlphaSlider.Value = TT.Options.Settings.MainTaskbarStyle.WindowsAccentAlpha;
                ColorizeBlurCheckBox.IsChecked = TT.Options.Settings.MainTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = TT.Options.Settings.MainTaskbarStyle.UseWindowsAccentColor;
            }
            else if (tb == "Maximized") {
                AccentStateComboBox.SelectedItem = (AccentState) TT.Options.Settings.MaximizedTaskbarStyle.AccentState;
                GradientColorPicker.SelectedColor = (Color) ColorConverter.ConvertFromString (TT.Options.Settings.MaximizedTaskbarStyle.GradientColor);
                WindowsAccentAlphaSlider.Value = TT.Options.Settings.MaximizedTaskbarStyle.WindowsAccentAlpha;
                ColorizeBlurCheckBox.IsChecked = TT.Options.Settings.MaximizedTaskbarStyle.Colorize;
                WindowsAccentColorCheckBox.IsChecked = TT.Options.Settings.MaximizedTaskbarStyle.UseWindowsAccentColor;
            }
        }

        #endregion Functions

        #region Control Handles
        private void StartStopButton_Click(object sender, RoutedEventArgs e) {
            if (RunApplyTask) {
                StartStopButton.Content = "Start";
                RunApplyTask = false;
            }
            else {
                StartStopButton.Content = "Stop";
                RunApplyTask = true;

                ApplyTask = new Task (ApplyToAllTaskbars);
                ApplyTask.Start ();
            }
        }

        private void AccentStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!WindowInitialized)
                return;
            SetAccentState ((AccentState) AccentStateComboBox.SelectedItem);
        }

        private void GradientColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (!WindowInitialized)
                return;

            SetTaskbarColor (GradientColorPicker.SelectedColor ?? Colors.White);
        }

        private void ColorizeBlurCheckBox_Changed(object sender, RoutedEventArgs e) {
            if (!WindowInitialized)
                return;
            SetAccentFlags (ColorizeBlurCheckBox.IsChecked ?? false);
        }

        private void SysTrayIcon_MouseClick(object sender, EventArgs e) {
            var me = (System.Windows.Forms.MouseEventArgs) e;
            if (me.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            SysTrayContextMenu.PlacementTarget = sender as Button;
            SysTrayContextMenu.IsOpen = true;
            Activate ();
        }

        private void WindowsAccentColorCheckBox_Changed(object sender, RoutedEventArgs e) {
            Globals.WindowsAccentColor = WindowsAccentColor.GetColorAsInt ();

            if (!WindowInitialized)
                return;

            var use = WindowsAccentColorCheckBox.IsChecked ?? false;
            SetUseAccentColor (use);
            GradientColorPicker.IsEnabled = !use;
        }

        private void UseMaximizedSettingsCheckBox_Changed(object sender, RoutedEventArgs e) {
            if (!WindowInitialized)
                return;
            TT.Options.Settings.UseDifferentSettingsWhenMaximized = UseMaximizedSettingsCheckBox.IsChecked ?? false;
            Taskbars.UpdateAllSettings ();
        }

        private void WindowsAccentAlphaSlider_DragCompleted(object sender, RoutedEventArgs e) {
            alphaDragStarted = false;
            SetWindowsAccentAlpha ((byte) WindowsAccentAlphaSlider.Value);
        }

        private void WindowsAccentAlphaSlider_DragStarted(object sender, RoutedEventArgs e) {
            alphaDragStarted = true;
        }

        private void WindowsAccentAlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (!WindowInitialized)
                return;
            if (!alphaDragStarted)
                SetWindowsAccentAlpha ((byte) WindowsAccentAlphaSlider.Value);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e) {
            if (MainGrid.RowDefinitions[2].Height == new GridLength (0)) {
                MainGrid.RowDefinitions[2].Height = new GridLength (90);
                Height += 90;
            }
            else {
                MainGrid.RowDefinitions[2].Height = new GridLength (0);
                Height -= 90;
            }
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e) {
            if (!WindowInitialized)
                return;

            TT.Options.Settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

            try {
                if (TT.Options.Settings.StartWithWindows) { rkApp.SetValue ("TaskbarTools", $"\"{MyPath}\""); }
                else { rkApp.DeleteValue ("TaskbarTools", false); }
            }
            catch (Exception ex) {
                MessageBox.Show (ex.Message, "Failed to Set Registry Key");
            }
        }

        private void EditSwitchButton_Click(object sender, RoutedEventArgs e) {
            if (TaskbarBeingEdited == "Main")
                SwitchTaskbarBeingEdited ("Maximized");
            else if (TaskbarBeingEdited == "Maximized")
                SwitchTaskbarBeingEdited ("Main");
        }

        #endregion Control Handles
    }
}