using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Logging;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal class ShaderManager
{
    private readonly D3DCompiler _compiler;
    private readonly ComPtr<ID3D11Device> _device;
    private readonly Dictionary<string, ComPtr<ID3D11VertexShader>> _vertexShadersCache = new();
    private readonly Dictionary<string, ComPtr<ID3D11InputLayout>> _vertexLayoutsCache = new();
    private readonly Dictionary<string, ComPtr<ID3D11PixelShader>> _pixelShadersCache = new();

    public ShaderManager(ComPtr<ID3D11Device> device, D3DCompiler compiler)
    {
        _device = device;
        _compiler = compiler;
    }

    #region Public Methods
    public unsafe void GetOrLoadVertexShaderAndLayout(IShaderFile shaderFile, InputElementDesc[] inputLayoutDesc, ref ComPtr<ID3D11VertexShader> vertexShader, ref ComPtr<ID3D11InputLayout> vertexLayout)
    {
        string shaderId = GetShaderId(shaderFile);
        bool vertexShaderFound = _vertexShadersCache.TryGetValue(shaderId, out ComPtr<ID3D11VertexShader> vertexShaderTmp);
        bool vertexLayoutFound = _vertexLayoutsCache.TryGetValue(shaderId, out ComPtr<ID3D11InputLayout> vertexLayoutTmp);

        if (!vertexShaderFound || !vertexLayoutFound)
        {
            if (shaderFile is ShaderCodeFile shaderCodeFile)
            {
                // Compilation et chargement du vertex shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderCodeFile))
                {
                    // Create vertex shader.
                    if (!vertexShaderFound)
                    {
                        vertexShaderTmp = CreateVertexShader(compilationBlob, shaderId);
                        _vertexShadersCache.Add(shaderId, vertexShaderTmp);
                    }

                    // Créer l’organisation des sommets
                    if (!vertexLayoutFound)
                    {
                        vertexLayoutTmp = CreateInputLayout(compilationBlob, inputLayoutDesc, shaderId);
                        _vertexLayoutsCache.Add(shaderId, vertexLayoutTmp);
                    }
                }
            }
            else if (shaderFile is ShaderByteCodeFile shaderByteCodeFile)
            {
                // Create vertex shader.
                if (!vertexShaderFound)
                {
                    vertexShaderTmp = CreateVertexShader(shaderByteCodeFile.Data, shaderId);
                    _vertexShadersCache.Add(shaderId, vertexShaderTmp);
                }

                // Créer l’organisation des sommets
                if (!vertexLayoutFound)
                {
                    vertexLayoutTmp = CreateInputLayout(shaderByteCodeFile.Data, inputLayoutDesc, shaderId);
                    _vertexLayoutsCache.Add(shaderId, vertexLayoutTmp);
                }
            }
        }
        vertexShader = vertexShaderTmp;
        vertexLayout = vertexLayoutTmp;
    }

    public unsafe ComPtr<ID3D11VertexShader> GetOrLoadVertexShader(IShaderFile shaderFile)
    {
        string shaderId = GetShaderId(shaderFile);
        bool vertexShaderFound = _vertexShadersCache.TryGetValue(shaderId, out ComPtr<ID3D11VertexShader> vertexShader);
        if (!vertexShaderFound)
        {
            if (shaderFile is ShaderCodeFile shaderCodeFile)
            {
                // Compilation et chargement du vertex shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderCodeFile))
                {
                    // Create vertex shader.
                    if (!vertexShaderFound)
                    {
                        vertexShader = CreateVertexShader(compilationBlob, shaderId);
                        _vertexShadersCache.Add(shaderId, vertexShader);
                    }
                }
            }
            else if (shaderFile is ShaderByteCodeFile shaderByteCodeFile)
            {
                // Create vertex shader.
                if (!vertexShaderFound)
                {
                    vertexShader = CreateVertexShader(shaderByteCodeFile.Data, shaderId);
                    _vertexShadersCache.Add(shaderId, vertexShader);
                }
            }
        }

        return vertexShader;
    }

    public unsafe ComPtr<ID3D11PixelShader> GetOrLoadPixelShader(IShaderFile shaderFile)
    {
        string shaderId = GetShaderId(shaderFile);
        bool pixelShaderFound = _pixelShadersCache.TryGetValue(shaderId, out ComPtr<ID3D11PixelShader> pixelShader);

        if (!pixelShaderFound)
        {
            if (shaderFile is ShaderCodeFile shaderCodeFile)
            {
                // Compilation et chargement du pixel shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderCodeFile))
                {
                    // Create pixel shader.
                    pixelShader = CreatePixelShader(compilationBlob, shaderId);
                    _pixelShadersCache.Add(shaderId, pixelShader);
                }
            }
            else if (shaderFile is ShaderByteCodeFile shaderByteCodeFile)
            {
                // Create pixel shader.
                pixelShader = CreatePixelShader(shaderByteCodeFile.Data, shaderId);
                _pixelShadersCache.Add(shaderId, pixelShader);
            }
        }
        return pixelShader;
    }
    #endregion

    private unsafe ComPtr<ID3D10Blob> Compile(ShaderCodeFile shaderDesc)
    {
        ComPtr<ID3D10Blob> compilationBlob = default;
        ComPtr<ID3D10Blob> compilationErrors = default;
        string filePath = shaderDesc.FilePath;
        byte[] shaderCode = File.ReadAllBytes(filePath);
        string entryPoint = shaderDesc.EntryPoint;
        string target = shaderDesc.Target;
        uint compilationFlags = shaderDesc.CompilationFlags;

        HResult hr = _compiler.Compile
        (
            in shaderCode[0],
            (nuint)shaderCode.Length,
            filePath,
            ref Unsafe.NullRef<D3DShaderMacro>(),
            ref Unsafe.NullRef<ID3DInclude>(),
            entryPoint,
            target,
            compilationFlags,
            0,
            ref compilationBlob,
            ref compilationErrors
        );

        // Check for compilation errors.
        if (hr.IsFailure)
        {
            if (compilationErrors.Handle is not null)
            {
                Log.Information(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
            }

            hr.Throw();
        }
        compilationErrors.Dispose();
        return compilationBlob;
    }

    private unsafe ComPtr<ID3D11VertexShader> CreateVertexShader(ComPtr<ID3D10Blob> compilationBlob, string name = "")
    {
        return CreateVertexShader(compilationBlob.GetBufferPointer(), compilationBlob.GetBufferSize(), name);
    }

    private unsafe ComPtr<ID3D11VertexShader> CreateVertexShader(byte[] byteCode, string name = "")
    {
        ComPtr<ID3D11VertexShader> shader;
        fixed (byte* byteCodePtr = byteCode)
        {
            shader = CreateVertexShader(byteCodePtr, (nuint)byteCode.Length, name);
        }
        return shader;
    }

    private unsafe ComPtr<ID3D11VertexShader> CreateVertexShader(void* byteCode, nuint byteCodeLength, string name = "")
    {
        ComPtr<ID3D11VertexShader> vertexShader = default;

        SilkMarshal.ThrowHResult
        (
            _device.CreateVertexShader
            (
                byteCode,
                byteCodeLength,
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref vertexShader
            )
        );

        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                IntPtr namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    vertexShader.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }

        return vertexShader;
    }

    private unsafe ComPtr<ID3D11InputLayout> CreateInputLayout(ComPtr<ID3D10Blob> compilationBlob, InputElementDesc[] inputLayoutDesc, string name = "")
    {
        return CreateInputLayout(compilationBlob.GetBufferPointer(), compilationBlob.GetBufferSize(), inputLayoutDesc, name);
    }

    private unsafe ComPtr<ID3D11InputLayout> CreateInputLayout(byte[] byteCode, InputElementDesc[] inputLayoutDesc, string name = "")
    {
        ComPtr<ID3D11InputLayout> shader;
        fixed (byte* byteCodePtr = byteCode)
        {
            shader = CreateInputLayout(byteCodePtr, (nuint)byteCode.Length, inputLayoutDesc, name);
        }
        return shader;
    }

    private unsafe ComPtr<ID3D11InputLayout> CreateInputLayout(void* byteCode, nuint byteCodeLength, InputElementDesc[] inputLayoutDesc, string name = "")
    {
        ComPtr<ID3D11InputLayout> inputLayout = default;
        fixed (InputElementDesc* inputLayoutDescPtr = inputLayoutDesc)
        {
            SilkMarshal.ThrowHResult
                (
                    _device.CreateInputLayout
                    (
                        inputLayoutDescPtr,
                        (uint)inputLayoutDesc.Length,
                        byteCode,
                        byteCodeLength,
                        ref inputLayout
                    )
                );
        }

        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                IntPtr namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    inputLayout.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return inputLayout;
    }

    private unsafe ComPtr<ID3D11PixelShader> CreatePixelShader(ComPtr<ID3D10Blob> compilationBlob, string name = "")
    {
        return CreatePixelShader(compilationBlob.GetBufferPointer(), compilationBlob.GetBufferSize(), name);
    }

    private unsafe ComPtr<ID3D11PixelShader> CreatePixelShader(byte[] byteCode, string name = "")
    {
        ComPtr<ID3D11PixelShader> shader;
        fixed (byte* byteCodePtr = byteCode)
        {
            shader = CreatePixelShader(byteCodePtr, (nuint)byteCode.Length, name);
        }
        return shader;
    }

    private unsafe ComPtr<ID3D11PixelShader> CreatePixelShader(void* byteCode, nuint byteCodeLength, string name = "")
    {
        ComPtr<ID3D11PixelShader> pixelShader = default;

        SilkMarshal.ThrowHResult
        (
            _device.CreatePixelShader
            (
                byteCode,
                byteCodeLength,
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref pixelShader
            )
        );

        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                IntPtr namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    pixelShader.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return pixelShader;
    }

    private static string GetShaderId(IShaderFile shaderFile)
    {
        if (!string.IsNullOrEmpty(shaderFile.Name))
        {
            return shaderFile.Name;
        }
        else
        {
            return shaderFile.FilePath;
        }
    }
}