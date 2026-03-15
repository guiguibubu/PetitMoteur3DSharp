using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal class InputLayout : IDisposable
{

    public InputLayoutDesc InputLayoutDesc { get; init; }
    public ComPtr<ID3D11InputLayout> InputLayoutRef { get; init; }

    private bool _disposed;

    public InputLayout(InputLayoutDesc inputLayoutDesc, ComPtr<ID3D11InputLayout> inputLayoutRef)
    {
        InputLayoutDesc = inputLayoutDesc;
        InputLayoutRef = inputLayoutRef;

        _disposed = false;
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
            InputLayoutRef.Dispose();
            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~InputLayout()
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

internal class InputLayoutDesc
{
    public InputElementDesc[] Data => _inputLayoutDesc;

    private InputElementDesc[] _inputLayoutDesc;
    private Dictionary<D3D11Semantics, uint> _inputLayoutKeys;

    public InputLayoutDesc(Dictionary<D3D11Semantics, InputElementDesc> inputLayoutDes)
    {
        _inputLayoutKeys = new Dictionary<D3D11Semantics, uint>();
        _inputLayoutDesc = new InputElementDesc[inputLayoutDes.Count];
        uint index = 0;
        foreach (D3D11Semantics semantics in inputLayoutDes.Keys)
        {
            _inputLayoutKeys.Add(semantics, index);
            _inputLayoutDesc[index] = inputLayoutDes[semantics];
            index++;
        }
    }
}

internal class InputLayoutDescBuilder
{
    private static readonly ConcurrentDictionary<D3D11Semantics, GlobalMemory> _semanticsChache = new ConcurrentDictionary<D3D11Semantics, GlobalMemory>();
    private Dictionary<InputLayoutBuilderKey, InputElementDesc> _inputLayout;

    public InputLayoutDescBuilder()
    {
        _inputLayout = new Dictionary<InputLayoutBuilderKey, InputElementDesc>();
    }

    public InputLayoutDesc Build()
    {
        return new InputLayoutDesc(_inputLayout.ToDictionary(p => p.Key.Semantic, p => p.Value));
    }

    public unsafe InputLayoutDescBuilder Add(D3D11Semantics semantic, Silk.NET.DXGI.Format format, uint inputSlot, uint alignedByteOffset)
    {
        return Add(semantic, semanticIndex: 0, format, inputSlot, alignedByteOffset);
    }

    public unsafe InputLayoutDescBuilder Add(D3D11Semantics semantic, uint semanticIndex, Silk.NET.DXGI.Format format, uint inputSlot, uint alignedByteOffset)
    {
        if (!_semanticsChache.TryGetValue(semantic, out GlobalMemory? semanticNameMemory))
        {
            semanticNameMemory = SilkMarshal.StringToMemory(semantic.ToSemanticString(), NativeStringEncoding.Ansi);
            _semanticsChache.TryAdd(semantic, semanticNameMemory);
        }
        InputLayoutBuilderKey key = new InputLayoutBuilderKey(semantic, semanticIndex);
        if (_inputLayout.ContainsKey(key))
        {
            throw new ArgumentException($"{semantic.ToSemanticString()} semantic already in InputLayout");
        }
        _inputLayout.Add(key, new InputElementDesc()
        {
            SemanticName = semanticNameMemory.AsPtr<byte>(),
            SemanticIndex = semanticIndex,
            Format = format,
            InputSlot = inputSlot,
            AlignedByteOffset = alignedByteOffset,
            InputSlotClass = InputClassification.PerVertexData,
            InstanceDataStepRate = 0
        });
        return this;
    }

    private readonly struct InputLayoutBuilderKey
    {
        public readonly D3D11Semantics Semantic;
        public readonly uint SemanticIndex;
        public InputLayoutBuilderKey(D3D11Semantics semantic, uint semanticIndex)
        {
            Semantic = semantic;
            SemanticIndex = semanticIndex;
        }
    }
}
