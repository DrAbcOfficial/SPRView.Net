#include "class_factory.h"
#include "com_ids.h"
#include "sprite_thumbnail_provider.h"

IFACEMETHODIMP CClassFactory::QueryInterface(REFIID riid, void** ppv) noexcept
{
    if (ppv == nullptr)
        return E_POINTER;
    if (riid == IID_IUnknown || riid == IID_IClassFactory)
        *ppv = static_cast<IClassFactory*>(this);
    else
    {
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    AddRef();
    return S_OK;
}

IFACEMETHODIMP_(ULONG) CClassFactory::AddRef() noexcept
{
    return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) CClassFactory::Release() noexcept
{
    const ULONG cRef = InterlockedDecrement(&m_cRef);
    if (cRef == 0)
        delete this;
    return cRef;
}

IFACEMETHODIMP CClassFactory::CreateInstance(IUnknown* punkOuter, REFIID riid, void** ppv) noexcept
{
    if (ppv == nullptr)
        return E_POINTER;
    *ppv = nullptr;
    if (punkOuter != nullptr)
        return CLASS_E_NOAGGREGATION;

    auto provider = new (std::nothrow) SpriteThumbnailProvider();
    if (provider == nullptr)
        return E_OUTOFMEMORY;
    const HRESULT hr = provider->QueryInterface(riid, ppv);
    provider->Release();
    return hr;
}

IFACEMETHODIMP CClassFactory::LockServer(BOOL fLock) noexcept
{
    fLock ? ModuleAddRef() : ModuleRelease();
    return S_OK;
}

STDAPI DllGetClassObject(REFCLSID clsid, REFIID riid, void** ppv)
{
    if (ppv == nullptr)
        return E_POINTER;
    *ppv = nullptr;
    if (clsid != CLSID_SpriteThumbnailProvider)
        return CLASS_E_CLASSNOTAVAILABLE;

    auto factory = new (std::nothrow) CClassFactory();
    if (factory == nullptr)
        return E_OUTOFMEMORY;
    const HRESULT hr = factory->QueryInterface(riid, ppv);
    factory->Release();
    return hr;
}
