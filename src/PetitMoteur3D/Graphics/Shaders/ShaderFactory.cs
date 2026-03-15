using System;

namespace PetitMoteur3D.Graphics.Shaders;

internal class ShaderFactory : IDisposable
{
    private ShaderManager _shaderManager;

    private bool _disposed;

    public ShaderFactory(ShaderManager shaderManager)
    {
        _shaderManager = shaderManager;
        _disposed = false;
    }

    public VertexShader CreateVertexShader(IShaderFile shaderFile, InputLayoutDesc inputLayoutDesc)
    {
        return _shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, inputLayoutDesc);
    }

    public PixelShader CreatePixelShader(IShaderFile shaderFile)
    {
        return _shaderManager.GetOrLoadPixelShader(shaderFile);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _shaderManager.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ShaderFactory()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
