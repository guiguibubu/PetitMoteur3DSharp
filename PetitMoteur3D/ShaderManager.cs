using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    internal class ShaderManager
    {
        private readonly D3DCompiler _compiler;
        private readonly ComPtr<ID3D11Device> _device;
        private readonly Dictionary<ShaderDesc, ComPtr<ID3D11VertexShader>> _vertexShadersCache = new();
        private readonly Dictionary<InputLayoutDesc, ComPtr<ID3D11InputLayout>> _vertexLayoutsCache = new();
        private readonly Dictionary<ShaderDesc, ComPtr<ID3D11PixelShader>> _pixelShadersCache = new();
        public ShaderManager(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            _device = device;
            _compiler = compiler;
        }

        #region Public Methods
        public unsafe void GetOrLoadVertexShaderAndLayout(ShaderDesc shaderDesc, InputElementDesc[] inputLayoutDesc, ref ComPtr<ID3D11VertexShader> vertexShader, ref ComPtr<ID3D11InputLayout> vertexLayout)
        {
            bool vertexShaderFound = _vertexShadersCache.TryGetValue(shaderDesc, out ComPtr<ID3D11VertexShader> vertexShaderTmp);
            InputLayoutDesc inputLayoutDescTmp = new() { ShaderDesc = shaderDesc, InputElementDescs = inputLayoutDesc };
            bool vertexLayoutFound = _vertexLayoutsCache.TryGetValue(inputLayoutDescTmp, out ComPtr<ID3D11InputLayout> vertexLayoutTmp);

            if (!vertexShaderFound || !vertexLayoutFound)
            {
                // Compilation et chargement du vertex shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderDesc))
                {
                    // Create vertex shader.
                    if (!vertexShaderFound)
                    {
                        vertexShaderTmp = CreateVertexShader(compilationBlob);
                        _vertexShadersCache.Add(shaderDesc, vertexShaderTmp);
                    }

                    // Créer l’organisation des sommets
                    if (!vertexLayoutFound)
                    {
                        vertexLayoutTmp = CreateInputLayout(compilationBlob, inputLayoutDesc);
                        _vertexLayoutsCache.Add(inputLayoutDescTmp, vertexLayoutTmp);
                    }
                }
            }
            vertexShader = vertexShaderTmp;
            vertexLayout = vertexLayoutTmp;
        }

        public unsafe ComPtr<ID3D11VertexShader> GetOrLoadVertexShader(ShaderDesc shaderDesc)
        {
            bool vertexShaderFound = _vertexShadersCache.TryGetValue(shaderDesc, out ComPtr<ID3D11VertexShader> vertexShader);
            if (!vertexShaderFound)
            {
                // Compilation et chargement du vertex shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderDesc))
                {
                    // Create vertex shader.
                    vertexShader = CreateVertexShader(compilationBlob);
                    _vertexShadersCache.Add(shaderDesc, vertexShader);

                }
            }

            return vertexShader;
        }

        public unsafe ComPtr<ID3D11PixelShader> GetOrLoadPixelShader(ShaderDesc shaderDesc)
        {
            bool pixelShaderFound = _pixelShadersCache.TryGetValue(shaderDesc, out ComPtr<ID3D11PixelShader> pixelShader);

            if (!pixelShaderFound)
            {
                // Compilation et chargement du pixel shader
                using (ComPtr<ID3D10Blob> compilationBlob = Compile(shaderDesc))
                {
                    // Create pixel shader.
                    pixelShader = CreatePixelShader(compilationBlob);
                    _pixelShadersCache.Add(shaderDesc, pixelShader);
                }
            }
            return pixelShader;
        }
        #endregion

        private unsafe ComPtr<ID3D10Blob> Compile(ShaderDesc shaderDesc)
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
                    System.Console.WriteLine(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
                }

                hr.Throw();
            }
            compilationErrors.Dispose();
            return compilationBlob;
        }

        private unsafe ComPtr<ID3D11VertexShader> CreateVertexShader(ComPtr<ID3D10Blob> compilationBlob)
        {
            ComPtr<ID3D11VertexShader> vertexShader = default;

            SilkMarshal.ThrowHResult
            (
                _device.CreateVertexShader
                (
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref vertexShader
                )
            );
            return vertexShader;
        }

        private unsafe ComPtr<ID3D11InputLayout> CreateInputLayout(ComPtr<ID3D10Blob> compilationBlob, InputElementDesc[] inputLayoutDesc)
        {
            ComPtr<ID3D11InputLayout> vertexLayout = default;
            fixed (InputElementDesc* inputLayoutDescPtr = inputLayoutDesc)
            {
                SilkMarshal.ThrowHResult
                    (
                        _device.CreateInputLayout
                        (
                            inputLayoutDescPtr,
                            (uint)inputLayoutDesc.Length,
                            compilationBlob.GetBufferPointer(),
                            compilationBlob.GetBufferSize(),
                            ref vertexLayout
                        )
                    );
            }
            return vertexLayout;
        }

        private unsafe ComPtr<ID3D11PixelShader> CreatePixelShader(ComPtr<ID3D10Blob> compilationBlob)
        {
            ComPtr<ID3D11PixelShader> pixelShader = default;

            SilkMarshal.ThrowHResult
            (
                _device.CreatePixelShader
                (
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref pixelShader
                )
            );
            return pixelShader;
        }

        public struct ShaderDesc
        {
            public string FilePath;
            public string EntryPoint;
            public string Target;
            public uint CompilationFlags;
        }

        private struct InputLayoutDesc
        {
            public ShaderDesc ShaderDesc;
            public InputElementDesc[] InputElementDescs;
        }
    }
}