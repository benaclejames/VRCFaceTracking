using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace VRCFaceTracking.Core;

/// <summary>
/// Windows-centric utilities class
/// </summary>
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

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteFile(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetFileAttributes(string lpFileName);
        
    public static readonly bool HasAdmin = !OperatingSystem.IsWindows() || new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        
    public static readonly string UserAccessibleDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VRCFaceTracking");
    public static readonly string PersistentDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCFaceTracking");
    public static readonly string CustomLibsDirectory = Path.Combine(PersistentDataDirectory, "CustomLibs");
    
    public static int GetRandomFreePort()
    {
        // Uses TcpListener to bind to a random port by attempting to bind to port zero and letting the OS assign one.
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    private const string K_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly Random Random = new();

    public static string GetRandomChars(int num) => new string(Enumerable.Repeat(K_CHARS, num).Select(s => s[Random.Next(s.Length)]).ToArray());
}
