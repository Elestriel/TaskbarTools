using System;
using System.Runtime.InteropServices;
using TaskbarTool.Enums;

namespace TaskbarTool.Structs
{

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
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
    public struct WinCompatTrData
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

    public struct WindowPlacement
    {
        public int length;
        public int flags;
        public WindowPlacementCommands showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }
    #endregion Structs
}
