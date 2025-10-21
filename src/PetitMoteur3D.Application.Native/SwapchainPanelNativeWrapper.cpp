#include "pch.h"
#include "SwapchainPanelNativeWrapper.h"

namespace SwapchainPanelNativeWrapper {
    int32_t Add(int32_t a, int32_t b) {
        return a + b;
    }

    int32_t SetSwapchain(IUnknown* panelIUnknown, IDXGISwapChain* swapChain)
    {
        //Get backing native interface for SwapChainPanel.
        Microsoft::WRL::ComPtr<ISwapChainPanelNative> panelNative;
        HRESULT castErrorCode = panelIUnknown->QueryInterface(__uuidof(ISwapChainPanelNative), &panelNative);
        if (FAILED(castErrorCode)) {
            return castErrorCode;
        }

        // Associate swap chain with SwapChainPanel.  This must be done on the UI thread.
        return panelNative->SetSwapChain(swapChain);
    }
}

int32_t SwapchainPanelNativeWrapper_Add(int32_t a, int32_t b) {
    return SwapchainPanelNativeWrapper::Add(a, b);
}

int32_t SwapchainPanelNativeWrapper_SetSwapchain(IUnknown* panelIUnknown, IDXGISwapChain* swapChain)
{
    //return 1;
    return SwapchainPanelNativeWrapper::SetSwapchain(panelIUnknown, swapChain);
}