using System;
using System.Runtime.InteropServices;

namespace PuckControl
{
    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
