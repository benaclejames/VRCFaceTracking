using System.Security.Principal;

namespace VRCFaceTracking;

[Obsolete("Please use VRCFaceTracking.Core.Utils instead")]
public class Utils
{
    public static uint TimeBeginPeriod(uint uMilliseconds) => Core.Utils.TimeBeginPeriod(uMilliseconds);

    public static uint TimeEndPeriod(uint uMilliseconds) => Core.Utils.TimeEndPeriod(uMilliseconds);

    // Proc memory read helpers
    public const int PROCESS_VM_READ = 0x0010;

    public static IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId) =>
        Core.Utils.OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);

    public static bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
        ref int lpNumberOfBytesRead) =>
        Core.Utils.ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, ref lpNumberOfBytesRead);

    public static bool DeleteFile(string lpFileName) => Core.Utils.DeleteFile(lpFileName);

    public static uint GetFileAttributes(string lpFileName) => Core.Utils.GetFileAttributes(lpFileName);

    public static readonly bool HasAdmin = Core.Utils.HasAdmin;

    public static readonly string UserAccessibleDataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VRCFaceTracking");

    public static readonly string PersistentDataDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCFaceTracking");

    public static readonly string CustomLibsDirectory = Path.Combine(PersistentDataDirectory, "CustomLibs");
}
