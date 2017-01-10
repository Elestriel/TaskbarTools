using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaskbarTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Invokes
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WinCompatTrData data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
        #endregion Invokes

        #region Enums
        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }
        #endregion Enums

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;

            public AccentPolicy(AccentState accentState, int accentFlags, int gradientColor, int animationId)
            {
                AccentState = accentState;
                AccentFlags = accentFlags;
                GradientColor = gradientColor;
                AnimationId = animationId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WinCompatTrData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;

            public WinCompatTrData(WindowCompositionAttribute attribute, IntPtr data, int sizeOfData)
            {
                Attribute = attribute;
                Data = data;
                SizeOfData = sizeOfData;
            }
        }
        #endregion Structs

        #region Declarations
        static Task ApplyTask;
        static bool RunTask = false;
        static AccentPolicy accentPolicy = new AccentPolicy();
        static System.Windows.Forms.NotifyIcon SysTrayIcon;
        ContextMenu SysTrayContextMenu;
        #endregion Declarations

        #region Initializations
        public MainWindow()
        {
            InitializeComponent();

            SysTrayContextMenu = this.FindResource("TrayContextMenu") as ContextMenu;

            SysTrayIcon = new System.Windows.Forms.NotifyIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("Resources/Mushroom1UP.ico", UriKind.Relative)).Stream;
            SysTrayIcon.Icon = new System.Drawing.Icon(iconStream);
            SysTrayIcon.Visible = true;
            SysTrayIcon.MouseClick += SysTrayIcon_MouseClick;
            SysTrayIcon.DoubleClick += 
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void PopulateComboBoxes()
        {
            AccentStateComboBox.ItemsSource = Enum.GetValues(typeof(AccentState)).Cast<AccentState>();
            AccentStateComboBox.SelectedIndex = 0;
        }

        private void LoadSettings()
        {
            try
            {
                AccentStateComboBox.SelectedItem = (AccentState)Properties.Settings.Default.AccentState;
                GradientColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.GradientColor);
                ColorizeBlurCheckBox.IsChecked = Properties.Settings.Default.Colorize;
            }
            catch (Exception ex)
            {
                AccentStateComboBox.SelectedIndex = 0;
                GradientColorPicker.SelectedColor = Color.FromArgb(127, 64, 127, 255);
                ColorizeBlurCheckBox.IsChecked = true;

                MessageBox.Show(ex.Message, "Error loading settings.");
            }
        }

        private void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.AccentState = (int)AccentStateComboBox.SelectedItem;
                Properties.Settings.Default.GradientColor = GradientColorPicker.SelectedColor.ToString();
                Properties.Settings.Default.Colorize = ColorizeBlurCheckBox.IsChecked ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving settings.");
                Console.WriteLine(ex.Message);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PopulateComboBoxes();
            LoadSettings();
        }
        #endregion Initializations

        #region Destructors
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SysTrayIcon.Dispose();
            SaveSettings();
        }

        private void CloseMainWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion Destructors

        #region Functions
        private void ApplyToAllTaskbars()
        {
            List<IntPtr> hWndList = new List<IntPtr>();

            hWndList.Add(FindWindow("Shell_TrayWnd", null));
            IntPtr otherBars = IntPtr.Zero;

            while (true)
            {
                otherBars = FindWindowEx(IntPtr.Zero, otherBars, "Shell_SecondaryTrayWnd", "");
                if (otherBars == IntPtr.Zero) { break; }
                else { hWndList.Add(otherBars); }
            }

            while (RunTask)
            {
                foreach (IntPtr hWnd in hWndList)
                {
                    SetWindowBlur(hWnd);
                    Thread.Sleep(10);
                }
            }
        }

        private void SetWindowBlur(IntPtr hWnd)
        {

            int sizeOfPolicy = Marshal.SizeOf(accentPolicy);
            IntPtr policyPtr = Marshal.AllocHGlobal(sizeOfPolicy);
            Marshal.StructureToPtr(accentPolicy, policyPtr, false);

            WinCompatTrData data = new WinCompatTrData(WindowCompositionAttribute.WCA_ACCENT_POLICY, policyPtr, sizeOfPolicy);

            SetWindowCompositionAttribute(hWnd, ref data);

            Marshal.FreeHGlobal(policyPtr);
        }

        #endregion Functions

        #region Control Handles
        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (RunTask)
            {
                StartStopButton.Content = "Start";
                RunTask = false;
            }
            else
            {
                StartStopButton.Content = "Stop";
                RunTask = true;
                ApplyTask = new Task(() => ApplyToAllTaskbars());
                ApplyTask.Start();
            }
        }

        private void AccentStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            accentPolicy.AccentState = (AccentState)AccentStateComboBox.SelectedItem;
        }

        private void GradientColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color gradientColor = GradientColorPicker.SelectedColor ?? Color.FromArgb(255, 255, 255, 255);
            accentPolicy.GradientColor = BitConverter.ToInt32(new byte[] { gradientColor.R, gradientColor.G, gradientColor.B, gradientColor.A }, 0);
        }

        private void ColorizeBlurCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (ColorizeBlurCheckBox.IsChecked == true) { accentPolicy.AccentFlags = 2; }
            else { accentPolicy.AccentFlags = 0; }
        }

        private void SysTrayIcon_MouseClick(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                SysTrayContextMenu.PlacementTarget = sender as Button;
                SysTrayContextMenu.IsOpen = true;
                this.Activate();
            }
        }
        #endregion Control Handles
    }
}