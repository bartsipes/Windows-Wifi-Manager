using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace WifiProfiles.UI
{
    /// <summary>
    /// Enforces a single instance.
    /// </summary>
    /// <remarks>
    /// This is where the magic happens.
    /// Start() tries to create a mutex.
    /// If it detects that another instance is already using the mutex, then it returns FALSE.
    /// Otherwise it returns TRUE.
    /// (Notice that a GUID is used for the mutex name, which is a little better than using the application name.)
    /// If another instance is detected, then you can use ShowFirstInstance() to show it
    /// (which will work as long as you override WndProc as shown above).
    /// ShowFirstInstance() broadcasts a message to all windows.
    /// The message is WM_SHOWFIRSTINSTANCE.
    /// (Notice that a GUID is used for WM_SHOWFIRSTINSTANCE.
    /// That allows you to reuse this code in multiple applications without getting
    /// strange results when you run them all at the same time.)
    /// 
    /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
    /// </remarks>
    static internal class SingleInstance
    {
        public static readonly int WM_SHOWFIRSTINSTANCE =
            NativeMethods.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);
        private static Mutex mutex;

        static public bool Start()
        {
            bool onlyInstance = false;

            // Limits app to a single instance across ALL SESSIONS (multiple users & terminal services)
            string mutexName = String.Format("Global\\{0}", ProgramInfo.AssemblyGuid);

            mutex = new Mutex(true, mutexName, out onlyInstance);
            return onlyInstance;
        }

        static public void ShowFirstInstance()
        {
            NativeMethods.PostMessage(
                (IntPtr)NativeMethods.HWND_BROADCAST,
                WM_SHOWFIRSTINSTANCE,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        static public void Stop()
        {
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Gets information about the application.
        /// </summary>
        /// <remarks>
        /// This class is just for getting information about the application.
        /// Each assembly has a GUID, and that GUID is useful to us in this application,
        /// so the most important thing in this class is the AssemblyGuid property.
        /// GetEntryAssembly() is used instead of GetExecutingAssembly(), so that you
        /// can put this code into a class library and still get the results you expect.
        /// (Otherwise it would return info on the DLL assembly instead of your application.)
        /// 
        /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
        /// </remarks>
        static class ProgramInfo
        {
            static internal string AssemblyGuid
            {
                get
                {
                    object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return String.Empty;
                    }
                    return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
                }
            }
            static internal string AssemblyTitle
            {
                get
                {
                    object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                    if (attributes.Length > 0)
                    {
                        AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                        if (titleAttribute.Title != "")
                        {
                            return titleAttribute.Title;
                        }
                    }
                    return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
                }
            }
        }

        /// <summary>
        /// A wrapper for various WinAPI functions.
        /// </summary>
        /// <remarks>
        /// This class is just a wrapper for your various WinApi functions.
        /// In this sample only the bare essentials are included.
        /// In my own WinApi class, I have all the WinApi functions that any
        /// of my applications would ever need.
        /// 
        /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
        /// </remarks>
        static class NativeMethods
        {
            internal const int HWND_BROADCAST = 0xffff;
            internal const int SW_SHOWNORMAL = 1;

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern int RegisterWindowMessage(string message);

            internal static int RegisterWindowMessage(string format, params object[] args)
            {
                string message = String.Format(format, args);
                return RegisterWindowMessage(message);
            }

            [DllImport("user32")]
            internal static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

            [DllImportAttribute("user32.dll")]
            internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImportAttribute("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);

            internal static void ShowToFront(IntPtr window)
            {
                ShowWindow(window, SW_SHOWNORMAL);
                SetForegroundWindow(window);
            }
        }
    }
}
