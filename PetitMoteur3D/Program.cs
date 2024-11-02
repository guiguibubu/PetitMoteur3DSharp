using ImGuiNET;
using PetitMoteur3D.DebugGui;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace PetitMoteur3D
{
    internal class Program
    {
        private static IWindow _window = default!;
        private static IInputContext _inputContext = default!;
        private static ImGuiController _imGuiController = default!;
        private static DeviceD3D11 _deviceD3D11 = default!;
        private static ShaderManager _shaderManager = default!;
        private static TextureManager _textureManager = default!;
        private static MeshLoader _meshLoader = default!;
        private static Scene _scene = default!;
        private static Matrix4X4<float> _matView = default;
        private static Matrix4X4<float> _matProj = default;

        private static bool _imGuiShowDemo = false;
        private static bool _debugToolKeyPressed = false;
        private static bool _showDebugTool = false;
        private static bool _showWireFrame = false;
        private static System.Numerics.Vector4 _backgroundColour = default!;

        private static Stopwatch _horloge = new Stopwatch();
        private const int IMAGESPARSECONDE = 60;
        private const double ECART_TEMPS = (1.0 / (double)IMAGESPARSECONDE) * 1000.0;

        private static bool _initAnimationFinished = false;
        static void Main(string[] args)
        {
            try
            {

                _window = WindowManager.Create();

                // Assign events.
                _window.Load += OnLoad;
                _window.Closing += OnClosing;
                _window.Render += OnRender;
                _window.FramebufferResize += OnFramebufferResize;

                // Run the window.
                _window.Run();

                //dispose the window, and its internal resources
                _window.Dispose();
            }
            catch (Exception ex)
            {
                Exception currentEx = ex;
                bool logFinished = false;
                do
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    if (currentEx.InnerException is not null)
                    {
                        currentEx = currentEx.InnerException;
                    }
                    else
                    {
                        logFinished = true;
                    }
                } while (!logFinished);
            }
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
            InitInput();
            System.Console.WriteLine("OnLoad InitInput Finished");
            InitDebugTools();
            System.Console.WriteLine("OnLoad InitDebugTools Finished");
        }

        private static void OnClosing()
        {
            System.Console.WriteLine("OnClosing");
            _imGuiController.Dispose();
            System.Console.WriteLine("OnClosing ImGuiController.Dispose Finished");
        }

        private static unsafe void OnRender(double elapsedTime)
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

                if (_showDebugTool)
                {
                    _imGuiController.Update((float)tempsEcoule);
                    _imGuiController.NewFrame();

                    ImGuiIOPtr io = ImGui.GetIO();

                    float f = 0.0f;
                    ImGui.Begin("Title : Hello, world!");
                    ImGui.Text("Hello, world!");
                    ImGui.SliderFloat("float", ref f, 0.0f, 1.0f);
                    ImGui.Text(string.Format("Application average {0} ms/frame ({1} FPS)", (1000.0f / io.Framerate).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), io.Framerate.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)));
                    bool colorChanged = ImGui.ColorEdit4("Background Color", ref _backgroundColour);     // Edit 4 floats representing a color
                    bool wireFrameChanged = ImGui.Checkbox("WireFrame", ref _showWireFrame);     // Edit 4 floats representing a color
                    ImGui.End();

                    // 1. Show the big demo window (Most of the sample code is in ImGui::ShowDemoWindow()! You can browse its code to learn more about Dear ImGui!).
                    if (_imGuiShowDemo)
                        ImGui.ShowDemoWindow(ref _imGuiShowDemo);

                    _imGuiController.Render();

                    if (colorChanged)
                    {
                        _deviceD3D11.SetBackgroundColour(_backgroundColour.X / _backgroundColour.W, _backgroundColour.Y / _backgroundColour.W, _backgroundColour.Z / _backgroundColour.W, _backgroundColour.W);
                    }

                    if (wireFrameChanged)
                    {
                        if (_showWireFrame && _deviceD3D11.GetRasterizerState().Handle != _deviceD3D11.WireFrameCullBackRS.Handle)
                        {
                            _deviceD3D11.SetRasterizerState(_deviceD3D11.WireFrameCullBackRS);
                        }
                        if (!_showWireFrame && _deviceD3D11.GetRasterizerState().Handle != _deviceD3D11.SolidCullBackRS.Handle)
                        {
                            _deviceD3D11.SetRasterizerState(_deviceD3D11.SolidCullBackRS);
                        }
                    }
                }
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
            _backgroundColour = _deviceD3D11.GetBackgroundColour().ToSystem();
            _textureManager = new TextureManager(_deviceD3D11.Device);
            _shaderManager = new ShaderManager(_deviceD3D11.Device, _deviceD3D11.ShaderCompiler);
            _meshLoader = new MeshLoader();
        }

        private static void InitScene()
        {
            _scene = new Scene();

            Bloc bloc = new(2.0f, 2.0f, 2.0f, _deviceD3D11, _shaderManager);
            bloc.SetTexture(_textureManager.GetOrLoadTexture("textures\\silk.png"));

            IReadOnlyList<SceneMesh>? meshes = _meshLoader.Load("models\\teapot.obj");
            ObjetMesh objetMesh = new(meshes[0], _deviceD3D11, _shaderManager);

            // _scene.AddObjet(bloc);
            _scene.AddObjet(objetMesh);

            BoundingBox boundingBox = objetMesh.Mesh.GetBoundingBox();
            float centerX = (boundingBox.Min.X + boundingBox.Max.X) / 2f;
            float centerY = (boundingBox.Min.Y + boundingBox.Max.Y) / 2f;
            float centerZ = (boundingBox.Min.Z + boundingBox.Max.Z) / 2f;
            float dimX = boundingBox.Max.X - boundingBox.Min.X;
            float dimY = boundingBox.Max.Y - boundingBox.Min.Y;
            float dimZ = boundingBox.Max.Z - boundingBox.Min.Z;
            Vector3D<float> sceneCenter = new(centerX, centerY, centerZ);
            Vector3D<float> sceneDim = new(dimX, dimY, dimZ);

            objetMesh.Mesh.AddTransform(Matrix4X4.CreateScale(4f / float.Max(float.Max(dimX, dimY), dimZ)));

            // Initialisation des matrices View et Proj
            // Dans notre cas, ces matrices sont fixes
            _matView = CreateLookAtLH(
                new Vector3D<float>(0.0f, 0.0f, -10.0f),
                new Vector3D<float>(0.0f, 0.0f, 0.0f),
                new Vector3D<float>(0.0f, 1.0f, 0.0f));
            float champDeVision = (float)(Math.PI / 4); // 45 degrés
            float largeurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.X;
            float hauteurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.Y;
            float aspectRatio = largeurEcran / hauteurEcran;
            float planRapproche = 2.0f;
            float planEloigne = 100.0f;
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

        private static void InitInput()
        {
            _inputContext = _window.CreateInput();
            _inputContext.Keyboards[0].KeyDown += (keyboard, key, i) =>
            {
                if (key == Key.F12 && !_debugToolKeyPressed)
                {
                    _debugToolKeyPressed = true;
                }
            };
            _inputContext.Keyboards[0].KeyUp += (keyboard, key, i) =>
                {
                    if (key == Key.F12 && _debugToolKeyPressed)
                    {
                        _showDebugTool = !_showDebugTool;
                        _debugToolKeyPressed = false;
                    }
                };
        }

        private static void InitDebugTools()
        {
            _imGuiController = new ImGuiController(_deviceD3D11, _window, _inputContext);
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
