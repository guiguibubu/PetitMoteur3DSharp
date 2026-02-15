using System;
using System.Numerics;

namespace PetitMoteur3D.Graphics;

internal class Material : IDisposable
{
    public Vector4 Ambient;
    public Vector4 Diffuse;
    public Vector4 Specular;
    public Vector4 Emission;
    public Vector4 Reflexion;
    public float Puissance;
    public bool Transparent;
    public Texture? DiffuseTexture;
    public Texture? NormalTexture;

    private bool _disposed;

    public Material()
    : this(Vector4.One, Vector4.One, Vector4.One, Vector4.One, Vector4.One, 1f, false)
    { }

    public Material(Vector4 ambient,
    Vector4 diffuse,
    Vector4 specular,
    Vector4 emission,
    Vector4 reflexion,
    float puissance,
    bool transparent)
    {
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Emission = emission;
        Reflexion = reflexion;
        Puissance = puissance;
        Transparent = transparent;

        _disposed = false;
    }

    public Material Clone()
    {
        return new Material(
            new Vector4(Ambient.X, Ambient.Y, Ambient.Z, Ambient.W),
            new Vector4(Diffuse.X, Diffuse.Y, Diffuse.Z, Diffuse.W),
            new Vector4(Specular.X, Specular.Y, Specular.Z, Specular.W),
            new Vector4(Emission.X, Emission.Y, Emission.Z, Emission.W),
            new Vector4(Reflexion.X, Reflexion.Y, Reflexion.Z, Reflexion.W),
            Puissance,
            Transparent
        );
    }


    ~Material()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SuppressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    // Dispose(bool disposing) executes in two distinct scenarios.
    // If disposing equals true, the method has been called directly
    // or indirectly by a user's code. Managed and unmanaged resources
    // can be disposed.
    // If disposing equals false, the method has been called by the
    // runtime from inside the finalizer and you should not reference
    // other objects. Only unmanaged resources can be disposed.
    private void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                DiffuseTexture?.Dispose();
                NormalTexture?.Dispose();
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.

            // Note disposing has been done.
            _disposed = true;
        }
    }
}