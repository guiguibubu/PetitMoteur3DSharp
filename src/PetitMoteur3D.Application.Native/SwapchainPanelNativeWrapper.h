#pragma once
#include <cstdint>
#include <wrl.h>
#include <microsoft.ui.xaml.media.dxinterop.h>

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)

EXTERN_DLL_EXPORT int32_t __cdecl SwapchainPanelNativeWrapper_Add(int32_t a, int32_t b);
EXTERN_DLL_EXPORT int32_t __cdecl SwapchainPanelNativeWrapper_SetSwapchain(IUnknown* panelIUnknown, IDXGISwapChain* swapChain);

