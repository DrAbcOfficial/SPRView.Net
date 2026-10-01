using System.Runtime.InteropServices;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// Self registration under HKCU so no elevation is required.
/// </summary>
internal static unsafe class RegistryOperations
{
    private const int HKEY_CURRENT_USER = unchecked((int)0x80000001);
    internal const int Hkcu = HKEY_CURRENT_USER;
    private const int KEY_SET_VALUE = 0x0002;
    private const int REG_OPTION_NON_VOLATILE = 0x0000;
    private const int REG_SZ = 1;
    private const int ERROR_SUCCESS = 0;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int RegCreateKeyExW(int hKey, string subKey, uint reserved, nint lpClass, uint options, int samDesired, nint securityAttributes, out nint phkResult, nint lpdwDisposition);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int RegSetValueExW(nint hKey, string? valueName, uint reserved, uint type, char* data, uint dataSize);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int RegDeleteTreeW(int hKey, string subKey);

    [DllImport("advapi32.dll")]
    private static extern int RegCloseKey(nint hKey);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetModuleFileNameW(nint hModule, char* filename, uint size);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetModuleHandleExW(uint flags, nint moduleName, out nint module);

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, nint dwItem1, nint dwItem2);

    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;
    private const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
    private const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

    public static void SetValue(int rootKey, string subKey, string? valueName, string data)
    {
        int error = RegCreateKeyExW(rootKey, subKey, 0, 0, REG_OPTION_NON_VOLATILE, KEY_SET_VALUE, 0, out nint key, 0);
        if (error != ERROR_SUCCESS)
            throw new InvalidOperationException($"RegCreateKeyExW failed: {error}");
        try
        {
            fixed (char* dataPtr = data)
            {
                error = RegSetValueExW(key, valueName, 0, REG_SZ, dataPtr, (uint)((data.Length + 1) * sizeof(char)));
            }
            if (error != ERROR_SUCCESS)
                throw new InvalidOperationException($"RegSetValueExW failed: {error}");
        }
        finally
        {
            RegCloseKey(key);
        }
    }

    public static void DeleteTree(int rootKey, string subKey)
    {
        RegDeleteTreeW(rootKey, subKey); // missing keys are not an error when unregistering
    }

    public static string GetOwnModulePath()
    {
        if (!GetModuleHandleExW(
                GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                (nint)(delegate* unmanaged<int>)&Mark, out nint module))
            throw new InvalidOperationException("GetModuleHandleExW failed");
        char[] buffer = new char[1024];
        fixed (char* bufferPtr = buffer)
        {
            uint length = GetModuleFileNameW(module, bufferPtr, (uint)buffer.Length);
            if (length == 0 || length == buffer.Length)
                throw new InvalidOperationException("GetModuleFileNameW failed");
            return new string(buffer, 0, (int)length);
        }
    }

    /// <summary>Address marker used to resolve this module's HINSTANCE.</summary>
    [UnmanagedCallersOnly]
    private static int Mark() => 0;

    public static void NotifyShellAssociationChanged() =>
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0);
}
