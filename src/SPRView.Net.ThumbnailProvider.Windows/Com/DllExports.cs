using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SPRView.Net.ThumbnailProvider.Windows.Com;

/// <summary>
/// The COM server entry points Explorer and regsvr32 interact with.
/// </summary>
internal static unsafe class DllExports
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
    private const int E_FAIL = unchecked((int)0x80004005);

    /// <summary>Never unload: the AOT runtime cannot be safely reinitialized.</summary>
    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
    public static int DllCanUnloadNow() => S_FALSE;

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    public static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv)
    {
        if (clsid == null || riid == null || ppv == null)
            return E_FAIL;
        *ppv = null;
        if (*clsid != ComIds.ClsidSpriteThumbnailProvider)
            return CLASS_E_CLASSNOTAVAILABLE;
        try
        {
            nint unknown = ComWrappersSource.Instance.GetOrCreateComInterfaceForObject(
                new SpriteThumbnailClassFactory(), CreateComInterfaceFlags.None);
            int hr = Marshal.QueryInterface(unknown, in *riid, out nint requested);
            Marshal.Release(unknown);
            if (hr == S_OK)
                *ppv = (void*)requested;
            return hr;
        }
        catch
        {
            return E_FAIL;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "DllRegisterServer")]
    public static int DllRegisterServer()
    {
        try
        {
            string modulePath = RegistryOperations.GetOwnModulePath();
            string clsidKey = $@"Software\Classes\CLSID\{ComIds.ClsidSpriteThumbnailProviderString}";
            RegistryOperations.SetValue(
                RegistryOperations.Hkcu, clsidKey, null, ComIds.ProviderDisplayName);
            RegistryOperations.SetValue(
                RegistryOperations.Hkcu, $@"{clsidKey}\InProcServer32", null, modulePath);
            RegistryOperations.SetValue(
                RegistryOperations.Hkcu, $@"{clsidKey}\InProcServer32", "ThreadingModel", "Apartment");
            RegistryOperations.SetValue(
                RegistryOperations.Hkcu, ComIds.ShellExThumbnailHandlerKey, null,
                ComIds.ClsidSpriteThumbnailProviderString);
            RegistryOperations.NotifyShellAssociationChanged();
            return S_OK;
        }
        catch
        {
            return E_FAIL;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "DllUnregisterServer")]
    public static int DllUnregisterServer()
    {
        try
        {
            RegistryOperations.DeleteTree(
                RegistryOperations.Hkcu,
                $@"Software\Classes\CLSID\{ComIds.ClsidSpriteThumbnailProviderString}");
            RegistryOperations.DeleteTree(RegistryOperations.Hkcu, ComIds.ShellExThumbnailHandlerKey);
            RegistryOperations.NotifyShellAssociationChanged();
            return S_OK;
        }
        catch
        {
            return E_FAIL;
        }
    }
}
