using System;
using System.IO;
using System.Xml.Serialization;

namespace TaskbarTool
{
    public static class TT
    {
        // Options
        public static Options Options = new Options();

        // My Documents
        private static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string FilePath = MyDocuments + "\\TaskbarTools\\Options.xml";

        public static void InitializeOptions()
        {
            if (!LoadOptions())
                AssignDefaults();
        }

        public static bool SaveOptions()
        {
            var serializer = new XmlSerializer(typeof(Options));

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                using (var fstream = new FileStream(FilePath, FileMode.Create))
                {
                    serializer.Serialize(fstream, Options);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool LoadOptions()
        {
            var serializer = new XmlSerializer(typeof(Options));
            if (!File.Exists(FilePath)) return false;

            try
            {
                using (var reader = new FileStream(FilePath, FileMode.Open))
                {
                    Options = serializer.Deserialize(reader) as Options;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Error loading Options.xml");
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private static void AssignDefaults()
        {
            Options.Settings = new OptionsSettings
            {
                StartMinimized = false,
                StartWhenLaunched = true,
                StartWithWindows = false,
                UseDifferentSettingsWhenMaximized = true,
                MainTaskbarStyle = new OptionsSettingsMainTaskbarStyle
                {
                    AccentState = 3,
                    GradientColor = "#804080FF",
                    Colorize = true,
                    UseWindowsAccentColor = true,
                    WindowsAccentAlpha = 127
                },
                MaximizedTaskbarStyle = new OptionsSettingsMaximizedTaskbarStyle
                {
                    AccentState = 2,
                    GradientColor = "#FF000000",
                    Colorize = false,
                    UseWindowsAccentColor = true,
                    WindowsAccentAlpha = 255
                }
            };


        }
    }

    #region XML Classes

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Options
    {

        private OptionsSettings settingsField;

        private byte versionField;

        /// <remarks/>
        public OptionsSettings Settings
        {
            get
            {
                return this.settingsField;
            }
            set
            {
                this.settingsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettings
    {

        private bool startMinimizedField;

        private bool startWhenLaunchedField;

        private bool startWithWindowsField;

        private bool useDifferentSettingsWhenMaximizedField;

        private OptionsSettingsMainTaskbarStyle mainTaskbarStyleField;

        private OptionsSettingsMaximizedTaskbarStyle maximizedTaskbarStyleField;

        /// <remarks/>
        public bool StartMinimized
        {
            get
            {
                return this.startMinimizedField;
            }
            set
            {
                this.startMinimizedField = value;
            }
        }

        /// <remarks/>
        public bool StartWhenLaunched
        {
            get
            {
                return this.startWhenLaunchedField;
            }
            set
            {
                this.startWhenLaunchedField = value;
            }
        }

        /// <remarks/>
        public bool StartWithWindows
        {
            get
            {
                return this.startWithWindowsField;
            }
            set
            {
                this.startWithWindowsField = value;
            }
        }

        /// <remarks/>
        public bool UseDifferentSettingsWhenMaximized
        {
            get
            {
                return this.useDifferentSettingsWhenMaximizedField;
            }
            set
            {
                this.useDifferentSettingsWhenMaximizedField = value;
            }
        }

        /// <remarks/>
        public OptionsSettingsMainTaskbarStyle MainTaskbarStyle
        {
            get
            {
                return this.mainTaskbarStyleField;
            }
            set
            {
                this.mainTaskbarStyleField = value;
            }
        }

        /// <remarks/>
        public OptionsSettingsMaximizedTaskbarStyle MaximizedTaskbarStyle
        {
            get
            {
                return this.maximizedTaskbarStyleField;
            }
            set
            {
                this.maximizedTaskbarStyleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMainTaskbarStyle
    {

        private byte accentStateField;

        private string gradientColorField;

        private bool colorizeField;

        private bool useWindowsAccentColorField;

        private byte windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState
        {
            get
            {
                return this.accentStateField;
            }
            set
            {
                this.accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        /// <remarks/>
        public string GradientColor
        {
            get
            {
                return this.gradientColorField;
            }
            set
            {
                this.gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize
        {
            get
            {
                return this.colorizeField;
            }
            set
            {
                this.colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor
        {
            get
            {
                return this.useWindowsAccentColorField;
            }
            set
            {
                this.useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha
        {
            get
            {
                return this.windowsAccentAlphaField;
            }
            set
            {
                this.windowsAccentAlphaField = value;
                if (UseWindowsAccentColor) { Taskbars.UpdateColor(); }
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionsSettingsMaximizedTaskbarStyle
    {

        private byte accentStateField;

        private string gradientColorField;

        private bool colorizeField;

        private bool useWindowsAccentColorField;

        private byte windowsAccentAlphaField;

        /// <remarks/>
        public byte AccentState
        {
            get
            {
                return this.accentStateField;
            }
            set
            {
                this.accentStateField = value;
                Taskbars.UpdateAccentState();
            }
        }

        /// <remarks/>
        public string GradientColor
        {
            get
            {
                return this.gradientColorField;
            }
            set
            {
                this.gradientColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public bool Colorize
        {
            get
            {
                return this.colorizeField;
            }
            set
            {
                this.colorizeField = value;
                Taskbars.UpdateAccentFlags();
            }
        }

        /// <remarks/>
        public bool UseWindowsAccentColor
        {
            get
            {
                return this.useWindowsAccentColorField;
            }
            set
            {
                this.useWindowsAccentColorField = value;
                Taskbars.UpdateColor();
            }
        }

        /// <remarks/>
        public byte WindowsAccentAlpha
        {
            get
            {
                return this.windowsAccentAlphaField;
            }
            set
            {
                this.windowsAccentAlphaField = value;
                if (UseWindowsAccentColor) { Taskbars.UpdateColor(); }
            }
        }
    }


    #endregion XML Classes
}