//Copyright (c) 2012 Stefan Moebius (mail@stefanmoebius.de)

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace xpsview
{
    public static class NativeCode
    {

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public const int MOD_ALT = 1;
        public const int IDHOT_SNAPDESKTOP = -2;
        public const int IDHOT_SNAPWINDOW = -1;
        public const int MOD_CONTROL = 2;

        public const uint WS_CHILD = 1073741824;
        public const uint WS_VISIBLE = 268435456;
        public const uint WS_CAPTION = 12582912;
        public const uint WS_SIZEBOX = 262144;
        public const uint WS_POPUP = 2147483648;
        public const uint WS_MAXIMIZEBOX = 65536;
        public const uint WS_MINIMIZEBOX = 131072;
        public const uint WS_SYSMENU = 524288;
        public const uint WS_OVERLAPPEDWINDOW =13565952;
        public const uint WS_MAXIMIZE = 16777216;
        public const uint WS_MINIMIZE = 536870912;

        public const string WC_MAGNIFIER = "Magnifier";
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const uint WS_EX_LAYERED = 524288;
        public const int LWA_ALPHA = 0x2;

        public const int DWM_EC_DISABLECOMPOSITION = 0;
        public const int DWM_EC_ENABLECOMPOSITION = 1;
        public const int WDA_MONITOR = 1;

        public const int RDW_FRAME = 0x0400;
        public const int RDW_UPDATENOW = 0x0100;
        public const int RDW_INVALIDATE = 0x0001;

        public const int VK_SNAPSHOT = 44;

        public const int SW_HIDE = 0;
        public const int SW_MAXIMIZE = 3;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOW = 5;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] uint dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] uint dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] uint dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr userData);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr RemoveProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr Hwnd);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int RegisterHotKey(IntPtr Hwnd, int ID, int Modifiers, int Key);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnregisterHotKey(IntPtr Hwnd, int ID);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, int flags);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, int dwAffinity);

        [DllImport("dwmapi.dll", SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll", SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int DwmEnableComposition(int compositionAction);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static IntPtr CreateWindow(int dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [DllImport("Magnification.dll", SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagInitialize();

        [DllImport("Magnification.dll", SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagUninitialize(); 

    }
}
