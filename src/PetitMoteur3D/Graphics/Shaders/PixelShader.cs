using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal class PixelShader : IDisposable
{
    public ComPtr<ID3D11PixelShader> ShaderInterface { get; init; }

    private bool disposedValue;

    public void Bind(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.PixelShaderStage.SetShader(ShaderInterface, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            ShaderInterface.Dispose();
            disposedValue = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~PixelShader()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
