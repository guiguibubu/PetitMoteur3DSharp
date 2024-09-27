using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;
using System.Diagnostics;

namespace PetitMoteur3D
{
    internal class Program
    {
        private static IWindow _window = default!;
        private static DeviceD3D11 _deviceD3D11 = default!;
        private static Scene _scene = default!;
        private static Matrix4X4<float> _matView = default;
        private static Matrix4X4<float> _matProj = default;

        private static Stopwatch _horloge = new Stopwatch();
        private const int IMAGESPARSECONDE = 60;
        private const double ECART_TEMPS = (1.0 / (double)IMAGESPARSECONDE) * 1000.0;

        private static bool _initAnimationFinished = false;
        static void Main(string[] args)
        {
            _window = WindowManager.Create();

            // Assign events.
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.FramebufferResize += OnFramebufferResize;

            // Run the window.
            _window.Run();

            //dispose the window, and its internal resources
            _window.Dispose();

        }

        private static void OnLoad()
        {
            System.Console.WriteLine("OnLoad");
            InitRendering();
            System.Console.WriteLine("OnLoad InitRendering Finished");
            InitScene();
            System.Console.WriteLine("OnLoad InitScene Finished");
            InitAnimation();
            System.Console.WriteLine("OnLoad InitAnimation Finished");
        }

        private static void OnUpdate(double elapsedTime)
        {
        }

        private static void OnRender(double elapsedTime)
        {
            double tempsEcoule = _horloge.ElapsedMilliseconds;
            // Est-il temps de rendre l’image ?
            if (tempsEcoule > ECART_TEMPS)
            {
                // Affichage optimisé
                _deviceD3D11.Present();
                // On relance l'horloge pour être certain de garder la fréquence d'affichage
                _horloge.Restart();
                // On prépare la prochaine image
                AnimeScene((float)tempsEcoule);
                // On rend l’image sur la surface de travail
                // (tampon d’arrière plan)
                RenderScene();
            }
        }

        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            // If the window resizes, we need to be sure to update the swapchain's back buffers.
            _deviceD3D11.Resize(newSize);
        }

        private static void BeginRender()
        {
            _deviceD3D11.BeforePresent();
        }

        private static void InitRendering()
        {
            _deviceD3D11 = new(_window);
        }

        private static void InitScene()
        {
            _scene = new Scene(new Bloc(2.0f, 2.0f, 2.0f, _deviceD3D11));

            // Initialisation des matrices View et Proj
            // Dans notre cas, ces matrices sont fixes
            _matView = CreateLookAtLH(
                new Vector3D<float>(0.0f, 0.0f, -10.0f),
                new Vector3D<float>(0.0f, 0.0f, 0.0f),
                new Vector3D<float>(0.0f, 1.0f, 0.0f));
            float champDeVision = (float)(Math.PI / 4); // 45 degrés
            Monitor.GetMainMonitor(_window);
            float largeurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.X;
            float hauteurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.Y;
            float aspectRatio = largeurEcran / hauteurEcran;
            float planRapproche = 2.0f;
            float planEloigne = 20.0f;
            _matProj = CreatePerspectiveFieldOfViewLH(
            champDeVision,
            aspectRatio,
            planRapproche,
            planEloigne);
        }

        private static void InitAnimation()
        {
            _horloge.Start();
            // première Image
            RenderScene();
            _initAnimationFinished = true;
        }

        private static void AnimeScene(float tempsEcoule)
        {
            _scene.Anime(tempsEcoule);
        }

        private static void RenderScene()
        {
            BeginRender();
            if (_initAnimationFinished)
            {
                Matrix4X4<float> matViewProj = _matView * _matProj;
                _scene.Draw(_deviceD3D11.DeviceContext, matViewProj);
            }
        }

        private static Matrix4X4<T> CreateLookAtLH<T>(Vector3D<T> cameraPosition, Vector3D<T> cameraTarget, Vector3D<T> cameraUpVector)
           where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            Vector3D<T> zaxis = Vector3D.Normalize(cameraTarget - cameraPosition);
            Vector3D<T> xaxis = Vector3D.Normalize(Vector3D.Cross(cameraUpVector, zaxis));
            Vector3D<T> yaxis = Vector3D.Cross(zaxis, xaxis);

            Matrix4X4<T> result = Matrix4X4<T>.Identity;

            result.M11 = xaxis.X;
            result.M12 = yaxis.X;
            result.M13 = zaxis.X;

            result.M21 = xaxis.Y;
            result.M22 = yaxis.Y;
            result.M23 = zaxis.Y;

            result.M31 = xaxis.Z;
            result.M32 = yaxis.Z;
            result.M33 = zaxis.Z;

            result.M41 = Scalar.Negate(Vector3D.Dot(xaxis, cameraPosition));
            result.M42 = Scalar.Negate(Vector3D.Dot(yaxis, cameraPosition));
            result.M43 = Scalar.Negate(Vector3D.Dot(zaxis, cameraPosition));

            return result;
        }

        public static Matrix4X4<T> CreatePerspectiveFieldOfViewLH<T>(T fieldOfView, T aspectRatio, T nearPlaneDistance, T farPlaneDistance)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            Matrix4X4<T> result = Matrix4X4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
            result.M31 = Scalar.Negate(result.M31);
            result.M32 = Scalar.Negate(result.M32);
            result.M33 = Scalar.Negate(result.M33);
            result.M34 = Scalar.Negate(result.M34);
            return result;
        }
    }
}
