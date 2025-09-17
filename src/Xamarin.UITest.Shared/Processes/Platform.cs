using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Xamarin.UITest.Shared.Processes
{
    internal class Platform : IPlatform
    {
        private static Platform _platform;
        private static readonly object PlatformLock = new();

        private Platform()
        { }

        public static Platform Instance
        {
            get
            {
                if (_platform == null)
                {
                    lock (PlatformLock)
                    {
                        _platform ??= new Platform();
                    }
                }
                return _platform;
            }
        }

        [DllImport("libc")] 
        static extern int uname(IntPtr buf);

        bool? _isUnix;
        bool? _isOSX;
        bool? _isWindows;

#if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("windows")]
#endif
        public bool IsWindows
        {
            get
            {
                if (!_isWindows.HasValue)
                {
                    _isWindows = Environment.OSVersion.Platform == PlatformID.Win32Windows
                        || Environment.OSVersion.Platform == PlatformID.Win32S
                        || Environment.OSVersion.Platform == PlatformID.Win32NT
                        || Environment.OSVersion.Platform == PlatformID.WinCE;
                }
                return _isWindows.Value;
            }
        }

        public bool IsOSXOrUnix { get { return IsUnix || IsOSX; } } 

        public bool IsUnix
        {
            get
            {
                if (!_isUnix.HasValue)
                {
                    _isUnix = IsOSX == false && IsWindows == false;
                }
                return _isUnix.Value;
            }
        }

        public bool IsOSX
        {
            get
            {
                if (!_isOSX.HasValue)
                {
                    // This code is based on a similar functionality in MonoDevelop:
                    // http://mono.1490590.n4.nabble.com/Howto-detect-os-td1549244.html

                    _isOSX = false;

                    if (!IsWindows)
                    {
                        var buf = IntPtr.Zero;

                        try
                        {
                            buf = Marshal.AllocHGlobal(8192);

                            if (uname(buf) == 0)
                            {
                                _isOSX = Marshal.PtrToStringAnsi(buf) == "Darwin";
                            }
                        }
                        finally
                        {
                            if (buf != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(buf);
                            }
                        }
                    }
                }
                return _isOSX.Value;
            }
        }
    }
}