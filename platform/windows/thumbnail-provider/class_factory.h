#pragma once

#include "module.h"

#include <unknwn.h>

// Standard COM class factory handing out SpriteThumbnailProvider instances.
class CClassFactory final : public IClassFactory
{
public:
    CClassFactory() noexcept : m_cRef(1) { ModuleAddRef(); }

    // IUnknown
    IFACEMETHODIMP QueryInterface(REFIID riid, void** ppv) noexcept override;
    IFACEMETHODIMP_(ULONG) AddRef() noexcept override;
    IFACEMETHODIMP_(ULONG) Release() noexcept override;

    // IClassFactory
    IFACEMETHODIMP CreateInstance(IUnknown* punkOuter, REFIID riid, void** ppv) noexcept override;
    IFACEMETHODIMP LockServer(BOOL fLock) noexcept override;

private:
    ~CClassFactory() { ModuleRelease(); }

    long m_cRef;
};
