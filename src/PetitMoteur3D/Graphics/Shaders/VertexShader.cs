using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal class VertexShader : IDisposable
{
    public ComPtr<ID3D11VertexShader> ShaderInterface { get; init; }
    public InputLayout InputLayout { get; init; }

    private bool _disposed;

    public VertexShader(ComPtr<ID3D11VertexShader> shaderInterface, InputLayout inputLayout)
    {
        ShaderInterface = shaderInterface;
        InputLayout = inputLayout;
        _disposed = false;
    }

    public void BindLayout(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.InputAssemblerStage.SetInputLayout(InputLayout.InputLayoutRef);
    }

    public void Bind(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.VertexShaderStage.SetShader(ShaderInterface, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            ShaderInterface.Dispose();
            InputLayout.Dispose();
            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~VertexShader()
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
