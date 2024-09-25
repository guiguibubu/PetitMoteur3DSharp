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

		private static int nbFrame = 0;
		private static int nbFrameMax = 60;
        private static void OnRender(double elapsedTime)
        {
            double tempsEcoule = _horloge.ElapsedMilliseconds;
            // Est-il temps de rendre l’image ?
            if (tempsEcoule > ECART_TEMPS && nbFrame < nbFrameMax)
            {
                if(_deviceD3D11.IsFrameCapturing() && nbFrame == 0)
                {
                    // _deviceD3D11.StartFrameCapture();
                }
                // Affichage optimisé
                _deviceD3D11.Present();
                if(nbFrame == 5)
                {
                    // _deviceD3D11.EndFrameCapture();
                }
                // On relance l'horloge pour être certain de garder la fréquence d'affichage
                _horloge.Restart();
                // On prépare la prochaine image
                AnimeScene((float)tempsEcoule);
                // On rend l’image sur la surface de travail
                // (tampon d’arrière plan)
                RenderScene();
				nbFrame++;
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
            _matView = Matrix4X4.CreateLookAt(
                new Vector3D<float>(0.0f, 0.0f, 10.0f),
                new Vector3D<float>(0.0f, 0.0f, 0.0f),
                new Vector3D<float>(0.0f, 1.0f, 0.0f));
            float champDeVision = (float)(Math.PI / 4); // 45 degrés
            float ratioDAspect = 1.0f; // horrible, il faudra corriger ça
            float planRapproche = 2.0f;
            float planEloigne = 20.0f;
            _matProj = Matrix4X4.CreatePerspectiveFieldOfView(
            champDeVision,
            ratioDAspect,
            planRapproche,
            planEloigne);
            _matProj = Matrix4X4<float>.Identity;
        }

        private static void InitAnimation()
        {
            _horloge.Start();
            // première Image
            RenderScene();
        }

        private static void AnimeScene(float tempsEcoule)
        {
            _scene.Anime(tempsEcoule);
        }

        private static void RenderScene()
        {
            BeginRender();
            Matrix4X4<float> matViewProj = _matProj * _matView;
            _scene.Draw(_deviceD3D11.DeviceContext, matViewProj);
        }
    }
}
