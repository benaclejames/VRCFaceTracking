using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace VRCFaceTracking
{
    public static class Utils
    {
        // Timer resolution helpers
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        // Proc memory read helpers
        public const int PROCESS_VM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        
        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        
        public static readonly string PersistentDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCFaceTracking");

        public static readonly Dictionary<Type, (char oscType, string configType)> TypeConversions =
            new Dictionary<Type, (char oscType, string configType)>
            {
                {typeof(bool), ('F', "Bool")},
                {typeof(float), ('f', "Float")},
                {typeof(int), ('i', "Int")},
            };
    }
}