using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using PetitMoteur3D.Camera;
using PetitMoteur3D.DebugGui;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Input;
using PetitMoteur3D.Logging;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

public class Engine
{
    public DeviceD3D11 DeviceD3D11 => _deviceD3D11;

    private readonly IWindow _window = default!;
    private readonly IInputContext? _inputContext = default!;

    private ImGuiController _imGuiController = default!;
    private DeviceD3D11 _deviceD3D11 = default!;
    private GraphicDeviceRessourceFactory _graphicDeviceRessourceFactory = default!;
    private GraphicPipelineFactory _graphicPipelineFactory = default!;

    private MeshLoader _meshLoader = default!;
    private Scene _scene = default!;
    private ICamera _camera = default!;
    private Matrix4x4 _matView = default;
    private Matrix4x4 _matProj = default;

    private bool _imGuiShowDemo = false;
    private bool _imGuiShowDebugLogs = false;
    private bool _imGuiShowEngineLogs = false;
    private bool _imGuiShowMetrics = false;
    private bool _debugToolKeyPressed = false;
    private bool _showDebugTool = false;
    private bool _showWireFrame = false;
    private bool _showScene = true;
    private Vector4 _backgroundColour = default!;

    private Stopwatch _horlogeEngine = new Stopwatch();
    private Stopwatch _horlogeScene = new Stopwatch();
    private Stopwatch _horlogeDebugTool = new Stopwatch();
    private const int IMAGESPARSECONDE_ENGINE = 60;
    private const int IMAGESPARSECONDE_SCENE = 60;
    private const int IMAGESPARSECONDE_DEBUGTOOL = 30;
    private const double ECART_TEMPS_ENGINE = (1.0 / (double)IMAGESPARSECONDE_ENGINE) * 1000.0;
    private const double ECART_TEMPS_SCENE = (1.0 / (double)IMAGESPARSECONDE_SCENE) * 1000.0;
    private const double ECART_TEMPS_DEBUGTOOL = (1.0 / (double)IMAGESPARSECONDE_DEBUGTOOL) * 1000.0;

    private readonly Int64 _memoryAtStartUp;
    private bool _initAnimationFinished = false;

    private Process _currentProcess = Process.GetCurrentProcess();
    private bool _onNativeDxPlatform;

    private bool _isInitializing = false;
    private bool _isInitialized = false;
    public event Action? Initialized;

    public ulong CurrentFrameCount = 0;

    public Engine(ref readonly EngineConfiguration conf)
    {
        _memoryAtStartUp = _currentProcess.WorkingSet64;
        _onNativeDxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _window = conf.Window;
        _inputContext = conf.InputContext;
    }

    public void Initialize()
    {
        if (_window.IsClosing)
        {
            Log.Information("[PetitMoteur3D] Initialize called window is closing");
            return;
        }
        if (!_window.IsInitialized)
        {
            Log.Information("[PetitMoteur3D] Initialize called but window not initialized");
            return;
        }
        // Assign events.
        _window.Closing += OnClosing;
        _window.Resize += OnResize;
        OnLoad();
    }

    public void Run()
    {
        Log.Information(string.Format("[PetitMoteur3D] Memory created at statup = {0} kB", _memoryAtStartUp / 1000));
        Log.Information(string.Format("[PetitMoteur3D] Memory created at Main begin = {0} kB", _currentProcess.WorkingSet64 / 1000));
        try
        {
            if (!_isInitialized)
            {
                Log.Information("[PetitMoteur3D] Run called but engine not initialized. Initialize before calling Run.");
                return;
            }
            if (_window.IsClosing)
            {
                Log.Information("[PetitMoteur3D] Run called window is closing");
                return;
            }
            if (!_window.IsInitialized)
            {
                Log.Information("[PetitMoteur3D] Run called but window not initialized");
                return;
            }

            // Run the window.
            _window.Run(MainLoop, this);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
                throw;
            }
        }
    }

    private void OnLoad()
    {
        Log.Information("[PetitMoteur3D] OnLoad");
        if (_isInitialized || _isInitializing)
        {
            Log.Information("[PetitMoteur3D] OnLoad Engine already initialized or initializing. No action will be performed");
            return;
        }
        _isInitializing = true;
        Log.Information(string.Format("[PetitMoteur3D] Memory created before init = {0} kB", _currentProcess.WorkingSet64 / 1000));
        InitRendering();
        Log.Information("[PetitMoteur3D] OnLoad InitRendering Finished");
        InitScene();
        Log.Information("[PetitMoteur3D] OnLoad InitScene Finished");
        InitAnimation();
        Log.Information("[PetitMoteur3D] OnLoad InitAnimation Finished");
        InitInput();
        Log.Information("[PetitMoteur3D] OnLoad InitInput Finished");
        InitDebugTools();
        Log.Information("[PetitMoteur3D] OnLoad InitDebugTools Finished");
        Log.Information(string.Format("[PetitMoteur3D] Memory created after init = {0} kB", _currentProcess.WorkingSet64 / 1000));
        _isInitializing = false;
        _isInitialized = true;
        Initialized?.Invoke();
    }

    private void OnClosing()
    {
        Log.Information("[PetitMoteur3D] OnClosing");
        _imGuiController?.Dispose();
        Log.Information("[PetitMoteur3D] OnClosing ImGuiController.Dispose Finished");
    }

    private unsafe void MainLoop()
    {
        if (_window.IsClosing)
        {
            Log.Information("[PetitMoteur3D] Main loop called window is closing");
            return;
        }
        if (!_window.IsInitialized)
        {
            Log.Information("[PetitMoteur3D] Main loop called but window not initialized");
            return;
        }
        CurrentFrameCount++;
        double tempsEcouleEngine = _horlogeEngine.ElapsedMilliseconds;
        try
        {
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
                        ImGui.Begin("Title : PetitMoteur3D (DebugTools)!");
                        ImGui.Text("Window : " + _window.GetType().Name);
                        ImGui.SliderFloat("float", ref f, 0.0f, 1.0f);
                        ImGui.Text(string.Format("Application average {0} ms/frame ({1} FPS)", (1000.0f / io.Framerate).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), io.Framerate.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)));
                        ImGui.Text(string.Format("Application memory usage {0} kB", _currentProcess.WorkingSet64 / 1000));
                        ImGui.Text(string.Format("Application (managed) heap usage {0} kB", GC.GetTotalMemory(false) / 1000));
                        bool runGc = ImGui.Button("Run GC");
                        bool colorChanged = ImGui.ColorEdit4("Background Color", ref _backgroundColour);     // Edit 4 floats representing a color
                        bool wireFrameChanged = ImGui.Checkbox("WireFrame", ref _showWireFrame);     // Edit bool
                        ImGui.Checkbox("Show Demo", ref _imGuiShowDemo);     // Edit bool
                        ImGui.Checkbox("Show Debug Logs", ref _imGuiShowDebugLogs);     // Edit bool
                        ImGui.Checkbox("Show Metrics", ref _imGuiShowMetrics);     // Edit bool
                        ImGui.Checkbox("Show Scene", ref _showScene);     // Edit bool
                        ImGui.Checkbox("Show Logs", ref _imGuiShowEngineLogs);     // Edit bool
                        ImGui.End();

                        // 1. Show the big demo window (Most of the sample code is in ImGui::ShowDemoWindow()! You can browse its code to learn more about Dear ImGui!).
                        if (_imGuiShowDemo)
                            ImGui.ShowDemoWindow(ref _imGuiShowDemo);

                        if (_imGuiShowDebugLogs)
                            ImGui.ShowDebugLogWindow();

                        if (_imGuiShowMetrics)
                            ImGui.ShowMetricsWindow();

                        if (_imGuiShowEngineLogs)
                        {
                            ImGui.Begin("PetitMoteur3D Logs");
                            ImGui.TextUnformatted("Bonjour !");
                            ImGui.End();
                        }
                        _imGuiController.Render(false);

                        if (colorChanged)
                        {
                            _deviceD3D11.SetBackgroundColour(_backgroundColour.X / _backgroundColour.W, _backgroundColour.Y / _backgroundColour.W, _backgroundColour.Z / _backgroundColour.W, _backgroundColour.W);
                        }

                        if (wireFrameChanged)
                        {
                            if (_showWireFrame)
                            {
                                _deviceD3D11.GetRasterizerState(out ComPtr<ID3D11RasterizerState> rasterizerState);
                                if (rasterizerState.Handle != _deviceD3D11.WireFrameCullBackRS.Handle)
                                {
                                    _deviceD3D11.SetRasterizerState(in _deviceD3D11.WireFrameCullBackRS);
                                }
                            }
                            if (!_showWireFrame)
                            {
                                _deviceD3D11.GetRasterizerState(out ComPtr<ID3D11RasterizerState> rasterizerState);
                                if (rasterizerState.Handle != _deviceD3D11.SolidCullBackRS.Handle)
                                {
                                    _deviceD3D11.SetRasterizerState(in _deviceD3D11.SolidCullBackRS);
                                }
                            }
                        }

                        if (runGc)
                        {
                            GC.Collect();
                        }
                    }
                    else
                    {
                        _imGuiController.Render(false);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Fatal(ex);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
                throw;
            }
        }
    }

    private void OnResize(Size newSize)
    {
        // If the window resizes, we need to be sure to update the swapchain's back buffers.
        _deviceD3D11.Resize(in newSize);

        // Update projection matrix
        float windowWidth = newSize.Width;
        float windowHeight = newSize.Height;
        float aspectRatio = windowWidth / windowHeight;
        float planRapproche = 2.0f;
        float planEloigne = 100.0f;
        _matProj = CreatePerspectiveFieldOfViewLH(
            _camera.ChampVision,
            aspectRatio,
            planRapproche,
            planEloigne
        );
    }

    private void BeginRender()
    {
        _deviceD3D11.BeforePresent();
    }

    private void InitRendering()
    {
        _deviceD3D11 = new DeviceD3D11(_window, !_onNativeDxPlatform);
        _deviceD3D11.GetBackgroundColour(out Vector4 backgroundColor);
        _backgroundColour = backgroundColor;
        _graphicDeviceRessourceFactory = new GraphicDeviceRessourceFactory(_deviceD3D11.Device, _deviceD3D11.ShaderCompiler);
        _graphicPipelineFactory = new GraphicPipelineFactory(_deviceD3D11.Device);
        _meshLoader = new MeshLoader();
    }

    private void InitScene()
    {
        //_camera = new FixedCamera(Vector3.Zero);
        //_camera = new ArcCamera(Vector3.Zero);
        _camera = new FreeCamera(_window);
        _camera.Move(-10 * Vector3.UnitZ);

        _scene = new Scene(_graphicDeviceRessourceFactory.BufferFactory, _camera);

        Bloc bloc1 = new(4.0f, 4.0f, 4.0f, _graphicDeviceRessourceFactory);
        bloc1.SetTexture(_graphicDeviceRessourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc1.SetNormalMapTexture(_graphicDeviceRessourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
        bloc1.Move(-4f, 0f, 0f);

        Bloc bloc2 = new(4.0f, 4.0f, 4.0f, _graphicDeviceRessourceFactory);
        bloc2.SetTexture(_graphicDeviceRessourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc2.Move(4f, 0f, 0f);

        IReadOnlyList<SceneMesh>? meshes = _meshLoader.Load("models\\teapot.obj");
        ObjetMesh objetMesh = new(meshes[0], _graphicDeviceRessourceFactory);
        BoundingBox boundingBox = objetMesh.Mesh.GetBoundingBox();
        float centerX = (boundingBox.Min.X + boundingBox.Max.X) / 2f;
        float centerY = (boundingBox.Min.Y + boundingBox.Max.Y) / 2f;
        float centerZ = (boundingBox.Min.Z + boundingBox.Max.Z) / 2f;
        float dimX = boundingBox.Max.X - boundingBox.Min.X;
        float dimY = boundingBox.Max.Y - boundingBox.Min.Y;
        float dimZ = boundingBox.Max.Z - boundingBox.Min.Z;
        Vector3 sceneCenter = new(centerX, centerY, centerZ);
        Vector3 sceneDim = new(dimX, dimY, dimZ);

        objetMesh.Mesh.AddTransform(Matrix4x4.CreateScale(4f / float.Max(float.Max(dimX, dimY), dimZ)));

        _scene.AddObjet(bloc1);
        _scene.AddObjet(bloc2);
        _scene.AddObjet(objetMesh);

        // Initialisation des matrices View et Proj
        // Dans notre cas, ces matrices sont fixes
        _camera.GetViewMatrix(out _matView);
        float windowWidth = _window.Size.Width;
        float windowHeight = _window.Size.Height;
        float aspectRatio = windowWidth / windowHeight;
        float planRapproche = 2.0f;
        float planEloigne = 100.0f;
        _matProj = CreatePerspectiveFieldOfViewLH(
            _camera.ChampVision,
            aspectRatio,
            planRapproche,
            planEloigne
        );
    }

    private void InitAnimation()
    {
        _horlogeEngine.Start();
        _horlogeScene.Start();
        _horlogeDebugTool.Start();
        // première Image
        RenderScene();
        _initAnimationFinished = true;
    }

    private void InitInput()
    {
        if (_inputContext is null)
        {
            Log.Information("[PetitMoteur3D] InputContext not initialized. Inputs will not be catched.");
            return;
        }
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
        _camera.InitInput(_inputContext);
    }

    private void InitDebugTools()
    {
        _imGuiController = new ImGuiController(_deviceD3D11, _graphicDeviceRessourceFactory, _graphicPipelineFactory, _window, _inputContext);
    }

    private void AnimeScene(float tempsEcoule)
    {
        _scene.Anime(tempsEcoule);
    }

    private void RenderScene()
    {
        BeginRender();
        if (_initAnimationFinished)
        {
            _camera.GetViewMatrix(out _matView);
            Matrix4x4 matViewProj = _matView * _matProj;
            _scene.Draw(in _deviceD3D11.DeviceContext, in matViewProj);
        }
    }

    public static Matrix4x4 CreatePerspectiveFieldOfViewLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        Matrix4x4 result = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        result.M31 = -result.M31;
        result.M32 = -result.M32;
        result.M33 = -result.M33;
        result.M34 = -result.M34;
        return result;
    }
}
