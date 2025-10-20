#include "pch.h"
#include "SwapchainPanelNativeWrapper.h"

namespace SwapchainPanelNativeWrapper {
    int32_t Add(int32_t a, int32_t b) {
        return a + b;
    }
}

int32_t SwapchainPanelNativeWrapper_Add(int32_t a, int32_t b) {
    return SwapchainPanelNativeWrapper::Add(a, b);
}