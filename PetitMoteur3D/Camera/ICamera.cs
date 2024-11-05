using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
    internal interface ICamera : ISceneObjet, IMovableObjet
    {
        /// <summary>
        /// Champ vision
        /// </summary>
        float ChampVision { get; }

        /// <summary>
        /// Get the current view matrix
        /// </summary>
        /// <returns></returns>
        Matrix4X4<float> GetViewMatrix();
    }
}