using ImGuiNET;
using PetitMoteur3D.DebugGui;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using PetitMoteur3D.Camera;

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
        private static ICamera _camera = default!;
        private static Matrix4X4<float> _matView = default;
        private static Matrix4X4<float> _matProj = default;

        private static bool _imGuiShowDemo = false;
        private static bool _imGuiShowDebugLogs = false;
        private static bool _imGuiShowMetrics = false;
        private static bool _debugToolKeyPressed = false;
        private static bool _showDebugTool = false;
        private static bool _showWireFrame = false;
        private static bool _showScene = true;
        private static System.Numerics.Vector4 _backgroundColour = default!;

        private static Stopwatch _horlogeEngine = new Stopwatch();
        private static Stopwatch _horlogeScene = new Stopwatch();
        private static Stopwatch _horlogeDebugTool = new Stopwatch();
        private const int IMAGESPARSECONDE_ENGINE = 60;
        private const int IMAGESPARSECONDE_SCENE = 60;
        private const int IMAGESPARSECONDE_DEBUGTOOL = 30;
        private const double ECART_TEMPS_ENGINE = (1.0 / (double)IMAGESPARSECONDE_ENGINE) * 1000.0;
        private const double ECART_TEMPS_SCENE = (1.0 / (double)IMAGESPARSECONDE_SCENE) * 1000.0;
        private const double ECART_TEMPS_DEBUGTOOL = (1.0 / (double)IMAGESPARSECONDE_DEBUGTOOL) * 1000.0;

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
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    throw;
                }
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
            double tempsEcouleEngine = _horlogeEngine.ElapsedMilliseconds;

            // Est-il temps de rendre l’image ?
            if (tempsEcouleEngine > ECART_TEMPS_ENGINE)
            {
                // Affichage optimisé
                _deviceD3D11.Present();
                // On relance l'horloge pour être certain de garder la fréquence d'affichage
                _horlogeEngine.Restart();
                // On prépare la prochaine image
                AnimeScene((float)tempsEcouleEngine);
                if (_showScene)
                {
                    double tempsEcouleScene = _horlogeScene.ElapsedMilliseconds;
                    if (tempsEcouleScene > ECART_TEMPS_SCENE)
                    {
                        _horlogeScene.Restart();
                        // On rend l’image sur la surface de travail
                        // (tampon d’arrière plan)
                        RenderScene();
                    }
                }
                else
                {
                    BeginRender();
                }

                if (_showDebugTool)
                {
                    double tempsEcouleDebugTool = _horlogeDebugTool.ElapsedMilliseconds;
                    // We create a new frame or reuse the last one
                    if (tempsEcouleDebugTool > ECART_TEMPS_DEBUGTOOL)
                    {
                        _imGuiController.CloseFrame();
                        _imGuiController.Update((float)tempsEcouleEngine / 1000.0f);
                        _imGuiController.NewFrame();

                        // On relance l'horloge pour être certain de garder la fréquence d'affichage
                        _horlogeDebugTool.Restart();

                        ImGuiIOPtr io = ImGui.GetIO();

                        float f = 0.0f;
                        ImGui.Begin("Title : Hello, world!");
                        ImGui.Text("Hello, world!");
                        ImGui.SliderFloat("float", ref f, 0.0f, 1.0f);
                        ImGui.Text(string.Format("Application average {0} ms/frame ({1} FPS)", (1000.0f * io.Framerate).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), io.Framerate.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)));
                        bool colorChanged = ImGui.ColorEdit4("Background Color", ref _backgroundColour);     // Edit 4 floats representing a color
                        bool wireFrameChanged = ImGui.Checkbox("WireFrame", ref _showWireFrame);     // Edit bool
                        ImGui.Checkbox("Show Demo", ref _imGuiShowDemo);     // Edit bool
                        ImGui.Checkbox("Show Debug Logs", ref _imGuiShowDebugLogs);     // Edit bool
                        ImGui.Checkbox("Show Metrics", ref _imGuiShowMetrics);     // Edit bool
                        ImGui.Checkbox("Show Scene", ref _showScene);     // Edit bool
                        ImGui.End();

                        // 1. Show the big demo window (Most of the sample code is in ImGui::ShowDemoWindow()! You can browse its code to learn more about Dear ImGui!).
                        if (_imGuiShowDemo)
                            ImGui.ShowDemoWindow(ref _imGuiShowDemo);

                        if (_imGuiShowDebugLogs)
                            ImGui.ShowDebugLogWindow();

                        if (_imGuiShowMetrics)
                            ImGui.ShowMetricsWindow();

                        _imGuiController.Render(false);

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
                    else
                    {
                        _imGuiController.Render(false);
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
            _camera = new FixedCamera(Vector3D<float>.Zero);
            _camera.Move(-10 * Vector3D<float>.UnitZ);

            _scene = new Scene(_deviceD3D11.Device, _camera);

            Bloc bloc1 = new(4.0f, 4.0f, 4.0f, _deviceD3D11, _shaderManager);
            bloc1.SetTexture(_textureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
            bloc1.SetNormalMapTexture(_textureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
            bloc1.Move(new Vector3D<float>(-4f, 0f, 0f));

            Bloc bloc2 = new(4.0f, 4.0f, 4.0f, _deviceD3D11, _shaderManager);
            bloc2.SetTexture(_textureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
            bloc2.Move(new Vector3D<float>(4f, 0f, 0f));

            IReadOnlyList<SceneMesh>? meshes = _meshLoader.Load("models\\teapot.obj");
            ObjetMesh objetMesh = new(meshes[0], _deviceD3D11, _shaderManager);
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

            _scene.AddObjet(bloc1);
            _scene.AddObjet(bloc2);
            _scene.AddObjet(objetMesh);

            // Initialisation des matrices View et Proj
            // Dans notre cas, ces matrices sont fixes
            _matView = _camera.GetViewMatrix();
            float largeurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.X;
            float hauteurEcran = Monitor.GetMainMonitor(_window).Bounds.Size.Y;
            float aspectRatio = largeurEcran / hauteurEcran;
            float planRapproche = 2.0f;
            float planEloigne = 100.0f;
            _matProj = CreatePerspectiveFieldOfViewLH(
                _camera.ChampVision,
                aspectRatio,
                planRapproche,
                planEloigne
            );
        }

        private static void InitAnimation()
        {
            _horlogeEngine.Start();
            _horlogeScene.Start();
            _horlogeDebugTool.Start();
            // première Image
            RenderScene();
            _initAnimationFinished = true;
        }

        private static void InitInput()
        {
            _inputContext = _window.CreateInput();
            IKeyboard keyboard = _inputContext.Keyboards[0];
            keyboard.KeyDown += (_, key, i) =>
            {
                if (key == Key.F11 && !_debugToolKeyPressed)
                {
                    _debugToolKeyPressed = true;
                }
            };
            keyboard.KeyUp += (_, key, i) =>
            {
                if (key == Key.F11 && _debugToolKeyPressed)
                {
                    _showDebugTool = !_showDebugTool;
                    _debugToolKeyPressed = false;
                }
            };
            keyboard.KeyChar += (_, c) =>
            {
                if (c == 'd')
                {
                    _camera.Move(Vector3D<float>.UnitX);
                }
                else if (c == 'q')
                {
                    _camera.Move(-Vector3D<float>.UnitX);
                }
            };
        }

        private static void InitDebugTools()
        {
            _imGuiController = new ImGuiController(_deviceD3D11, _shaderManager, _window, _inputContext);
            //_imGuiController = new ImGuiController(new NoOpImGuiBackendRenderer(), _window, _inputContext);
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
                _matView = _camera.GetViewMatrix();
                Matrix4X4<float> matViewProj = _matView * _matProj;
                _scene.Draw(_deviceD3D11.DeviceContext, matViewProj);
            }
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
