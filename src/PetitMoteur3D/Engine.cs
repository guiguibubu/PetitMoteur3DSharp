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
using PetitMoteur3D.Graphics.Shaders;
using PetitMoteur3D.Input;
using PetitMoteur3D.Logging;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

public class Engine
{
    public D3D11GraphicPipeline GraphicPipeline => _graphicPipeline;

    private readonly IWindow _window;
    private readonly IInputContext? _inputContext;

    private ImGuiController _imGuiController;
    private D3D11GraphicDevice _graphicDevice;
    private D3D11GraphicPipeline _graphicPipeline;

    private Scene _scene;
    private ICamera _activeCamera;
    private ICamera _gameCamera;
    private FixedCamera _debugCamera;

    private bool _imGuiShowDemo;
    private bool _imGuiShowDebugLogs;
    private bool _imGuiShowEngineLogs;
    private bool _imGuiShowGameCameraOptions;
    private bool _imGuiShowDebugCameraOptions;
    private bool _imGuiShowMetrics;
    private bool _debugToolKeyPressed;
    private bool _showDebugTool;
    private bool _showWireFrame;
    private bool _showScene;
    private bool _showShadow;
    private bool _showDepthTest;
    private bool _isShadowOrthographique;
    private bool _isCameraOrthographique;
    private bool _useDebugCamera;
    private Vector4 _backgroundColour;

    private readonly Stopwatch _horlogeEngine;
    private readonly Stopwatch _horlogeScene;
    private readonly Stopwatch _horlogeDebugTool;

    private const int IMAGESPARSECONDE_ENGINE = 60;
    private const int IMAGESPARSECONDE_SCENE = 60;
    private const int IMAGESPARSECONDE_DEBUGTOOL = 30;
    private const double ECART_TEMPS_ENGINE = (1.0 / (double)IMAGESPARSECONDE_ENGINE) * 1000.0;
    private const double ECART_TEMPS_SCENE = (1.0 / (double)IMAGESPARSECONDE_SCENE) * 1000.0;
    private const double ECART_TEMPS_DEBUGTOOL = (1.0 / (double)IMAGESPARSECONDE_DEBUGTOOL) * 1000.0;

    private readonly Int64 _memoryAtStartUp;
    private bool _initAnimationFinished;

    private Process _currentProcess = Process.GetCurrentProcess();
    private bool _onNativeDxPlatform;

    private bool _isInitializing;
    private bool _isInitialized;
    public event Action? Initialized;

    public ulong CurrentFrameCount;

    public Engine(EngineConfiguration conf)
    {
        _memoryAtStartUp = _currentProcess.WorkingSet64;
        _onNativeDxPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _window = conf.Window;
        _inputContext = conf.InputContext;

        _imGuiController = default!;
        _graphicDevice = default!;
        _graphicPipeline = default!;

        _backgroundColour = default;

        _isInitializing = false;
        _isInitialized = false;
        _initAnimationFinished = false;
        CurrentFrameCount = 0;

        _imGuiShowDemo = false;
        _imGuiShowDebugLogs = false;
        _imGuiShowEngineLogs = false;
        _imGuiShowDebugCameraOptions = false;
        _imGuiShowGameCameraOptions = false;
        _imGuiShowMetrics = false;
        _debugToolKeyPressed = false;
        _showDebugTool = false;
        _showWireFrame = false;
        _showScene = true;
        _showShadow = true;
        _isShadowOrthographique = true;
        _isCameraOrthographique = false;
        _useDebugCamera = false;

        _horlogeEngine = new Stopwatch();
        _horlogeScene = new Stopwatch();
        _horlogeDebugTool = new Stopwatch();

        _scene = default!;
        _activeCamera = default!;
        _gameCamera = default!;
        _debugCamera = default!;
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
        _graphicPipeline?.Dispose();
        _graphicDevice?.Dispose();
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
        double elapsedTimeEngine = _horlogeEngine.ElapsedMilliseconds;
        try
        {
            // Est-il temps de rendre l’image ?
            if (elapsedTimeEngine > ECART_TEMPS_ENGINE)
            {
                // Affichage optimisé
                _graphicPipeline.Present();
                // On relance l'horloge pour être certain de garder la fréquence d'affichage
                _horlogeEngine.Restart();
                // On prépare la prochaine image
                AnimeScene((float)elapsedTimeEngine);
                if (_showScene)
                {
                    double elapsedTimeScene = _horlogeScene.ElapsedMilliseconds;
                    if (elapsedTimeScene > ECART_TEMPS_SCENE)
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
                    double elapsedTimeDebugTool = _horlogeDebugTool.ElapsedMilliseconds;
                    // We create a new frame or reuse the last one
                    if (elapsedTimeDebugTool > ECART_TEMPS_DEBUGTOOL)
                    {
                        _imGuiController.CloseFrame();
                        _imGuiController.Update((float)elapsedTimeEngine / 1000.0f);
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
                        ImGui.Checkbox("Show Shadow", ref _showShadow);     // Edit bool
                        ImGui.Checkbox("Show DepthTest", ref _showDepthTest);     // Edit bool
                        ImGui.Checkbox("Show Shadow Orthographique", ref _isShadowOrthographique);     // Edit bool
                        ImGui.Checkbox("Show Debug Camera Options", ref _imGuiShowDebugCameraOptions);     // Edit bool
                        ImGui.Checkbox("Show Game Camera Options", ref _imGuiShowGameCameraOptions);     // Edit bool
                        ImGui.Checkbox("Show Logs", ref _imGuiShowEngineLogs);     // Edit bool
                        ImGui.End();

                        // 1. Show the big demo window (Most of the sample code is in ImGui::ShowDemoWindow()! You can browse its code to learn more about Dear ImGui!).
                        if (_imGuiShowDemo)
                            ImGui.ShowDemoWindow(ref _imGuiShowDemo);

                        if (_imGuiShowDebugLogs)
                            ImGui.ShowDebugLogWindow();

                        if (_imGuiShowMetrics)
                            ImGui.ShowMetricsWindow();

                        if (_imGuiShowDebugCameraOptions)
                        {
                            ImGui.Begin("PetitMoteur3D Debug Camera");
                            ImGui.Checkbox("Use debug camera", ref _useDebugCamera);     // Edit bool
                            Vector3 position = _debugCamera.Position;
                            ImGui.SliderFloat("X", ref position.X, -100f, 100f);
                            ImGui.SliderFloat("Y", ref position.Y, -100f, 100f);
                            ImGui.SliderFloat("Z", ref position.Z, -100f, 100f);
                            _debugCamera.SetPosition(in position);

                            Vector3 target = _debugCamera.Target;
                            ImGui.SliderFloat("Target X", ref target.X, -100f, 100f);
                            ImGui.SliderFloat("Target Y", ref target.Y, -100f, 100f);
                            ImGui.SliderFloat("Target Z", ref target.Z, -100f, 100f);
                            _debugCamera.SetTarget(in target);
                            ImGui.End();
                        }

                        if (_imGuiShowGameCameraOptions)
                        {
                            ImGui.Begin("PetitMoteur3D Game Camera");
                            ImGui.Checkbox("Orthographique", ref _isCameraOrthographique);     // Edit bool
                            FrustrumView gameFrustrumView = _gameCamera.FrustrumView;
                            gameFrustrumView.IsOrthographique = _isCameraOrthographique;
                            float width = gameFrustrumView.Width;
                            ImGui.SliderFloat("width", ref width, 400.0f, 2040f);
                            gameFrustrumView.Width = width;
                            float height = gameFrustrumView.Height;
                            ImGui.SliderFloat("height", ref height, 400.0f, 2040f);
                            gameFrustrumView.Height = height;
                            float fieldOfView = gameFrustrumView.FieldOfView;
                            ImGui.SliderFloat("fieldOfView", ref fieldOfView, 0.1f, (float)Math.PI - 0.1f);
                            gameFrustrumView.FieldOfView = fieldOfView;
                            float nearPlaneDistance = gameFrustrumView.NearPlaneDistance;
                            float farPlaneDistance = gameFrustrumView.FarPlaneDistance;
                            ImGui.SliderFloat("nearPlaneDistance", ref nearPlaneDistance, 0.1f, farPlaneDistance - 0.1f);
                            ImGui.SliderFloat("farPlaneDistance", ref farPlaneDistance, nearPlaneDistance + 0.1f, 500f);
                            gameFrustrumView.NearPlaneDistance = nearPlaneDistance;
                            gameFrustrumView.FarPlaneDistance = farPlaneDistance;
                            ImGui.End();
                        }

                        if (_imGuiShowEngineLogs)
                        {
                            ImGui.Begin("PetitMoteur3D Logs");
                            ImGui.TextUnformatted("Bonjour !");
                            ImGui.End();
                        }
                        _imGuiController.Render(false);

                        if (colorChanged)
                        {
                            _graphicPipeline.SetBackgroundColour(_backgroundColour.X / _backgroundColour.W, _backgroundColour.Y / _backgroundColour.W, _backgroundColour.Z / _backgroundColour.W, _backgroundColour.W);
                        }

                        if (wireFrameChanged)
                        {
                            ComPtr<ID3D11RasterizerState> rasterizerState = _scene.RasterizerState;
                            if (_showWireFrame)
                            {
                                if (rasterizerState.Handle != _graphicPipeline.WireFrameCullBackRS.Handle)
                                {
                                    _scene.RasterizerState = _graphicPipeline.WireFrameCullBackRS;
                                }
                            }
                            else
                            {
                                if (rasterizerState.Handle != _graphicPipeline.SolidCullBackRS.Handle)
                                {
                                    _scene.RasterizerState = _graphicPipeline.SolidCullBackRS;
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
        _graphicPipeline.Resize(in newSize);

        // Update projection matrix
        float windowWidth = newSize.Width;
        float windowHeight = newSize.Height;
        FrustrumView gameFrustrumView = _gameCamera.FrustrumView;
        gameFrustrumView.Update(_gameCamera.ChampVision,
            windowWidth,
            windowHeight,
            gameFrustrumView.NearPlaneDistance,
            gameFrustrumView.FarPlaneDistance);

        FrustrumView debugFrustrumView = _debugCamera.FrustrumView;
        debugFrustrumView.Update(_gameCamera.ChampVision,
            windowWidth,
            windowHeight,
            debugFrustrumView.NearPlaneDistance,
            debugFrustrumView.FarPlaneDistance);

        _scene.OnScreenResize(newSize);
    }

    private void BeginRender()
    {
        _graphicPipeline.BeforePresent();
    }

    private void InitRendering()
    {
        _graphicDevice = new D3D11GraphicDevice(!_onNativeDxPlatform);
        _graphicPipeline = new D3D11GraphicPipeline(_graphicDevice, _window);
        _graphicPipeline.GetBackgroundColour(out Vector4 backgroundColor);
        _backgroundColour = backgroundColor;
    }

    private void InitScene()
    {
        float windowWidth = _window.Size.Width;
        float windowHeight = _window.Size.Height;
        float champVision = (float)(Math.PI / 4d);
        _debugCamera = new FixedCamera(target: new Vector3(0f, 2f, 0f), champVision)
        {
            FrustrumView = new FrustrumView(
                champVision,
                windowWidth,
                windowHeight,
                nearPlaneDistance: 2f,
                farPlaneDistance: 150f,
                isOrthographic: false
            )
        };

        FrustrumView gameFrustrumView = new(
            champVision,
            windowWidth,
            windowHeight,
            nearPlaneDistance: 2f,
            farPlaneDistance: 15f,
            isOrthographic: _isCameraOrthographique
        );
        _gameCamera = new FixedCamera(target: new Vector3(0f, 2f, 0f), champVision)
        //_gameCamera = new ArcCamera(Vector3.Zero, champVision)
        //_gameCamera = new FreeCamera(_window, champVision)
        {
            FrustrumView = gameFrustrumView
        };
        _gameCamera.Move(0f, 2f, -10f);

        _scene = InitDefaultScene(_graphicDevice.RessourceFactory, _graphicPipeline.ShaderFactory, _gameCamera, _window);
        // Set default rasterizer state
        _scene.RasterizerState = _graphicPipeline.SolidCullBackRS;
    }

    private static Scene InitDefaultScene(GraphicDeviceRessourceFactory ressourceFactory, RenderPassFactory shaderFactory, ICamera gameCamera, IWindow window)
    {
        Scene scene = new(ressourceFactory, gameCamera, window.Size);
        Bloc bloc1 = new(4.0f, 4.0f, 4.0f, ressourceFactory, shaderFactory);
        bloc1.SetTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc1.SetNormalMapTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
        bloc1.Move(-4f, 2f, 0f);

        Bloc bloc2 = new(4.0f, 4.0f, 4.0f, ressourceFactory, shaderFactory);
        bloc2.SetTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc2.SetNormalMapTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
        bloc2.Move(4f, 2f, 0f);

        Bloc bloc3 = new(4.0f, 4.0f, 4.0f, ressourceFactory, shaderFactory);
        bloc3.SetTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc3.SetNormalMapTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
        bloc3.Move(-4f, 2f, 4f);

        Bloc bloc4 = new(4.0f, 4.0f, 4.0f, ressourceFactory, shaderFactory);
        bloc4.SetTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall.jpg"));
        bloc4.SetNormalMapTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\brickwall_normal.jpg"));
        bloc4.Move(4f, 2f, 4f);

        MeshLoader meshLoader = new();
        IReadOnlyList<SceneMesh>? meshes = meshLoader.Load("models\\teapot.obj");

        SceneMesh rootMesh = meshes[0];
        BoundingBox boundingBox = rootMesh.GetBoundingBox();
        float centerX = (boundingBox.Min.X + boundingBox.Max.X) / 2f;
        float centerY = (boundingBox.Min.Y + boundingBox.Max.Y) / 2f;
        float centerZ = (boundingBox.Min.Z + boundingBox.Max.Z) / 2f;
        float dimX = boundingBox.Max.X - boundingBox.Min.X;
        float dimY = boundingBox.Max.Y - boundingBox.Min.Y;
        float dimZ = boundingBox.Max.Z - boundingBox.Min.Z;

        rootMesh.AddTransform(Matrix4x4.CreateScale(4f / float.Max(float.Max(dimX, dimY), dimZ)));

        ObjetMesh objetMesh = new(rootMesh, ressourceFactory, shaderFactory);
        Vector3 sceneCenter = new(centerX, centerY, centerZ);
        Vector3 sceneDim = new(dimX, dimY, dimZ);

        objetMesh.Move(0f, 2f, 0f);

        Plane ground = new(10f, 10f, ressourceFactory, shaderFactory);
        ground.SetTexture(ressourceFactory.TextureManager.GetOrLoadTexture("textures\\silk.png"));
        ground.Rotate(Vector3.UnitX, (float)(Math.PI / 2f));

        scene.AddObjet(bloc1);
        scene.AddObjet(bloc2);
        scene.AddObjet(bloc3);
        scene.AddObjet(bloc4);
        scene.AddObjet(objetMesh);
        scene.AddObjet(ground);

        return scene;
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
        _gameCamera.InitInput(_inputContext);
    }

    private void InitDebugTools()
    {
        _imGuiController = new ImGuiController(_graphicPipeline, _window, _inputContext);
    }

    private void AnimeScene(float elapsedTime)
    {
        _gameCamera.Update(elapsedTime);
        _scene.Anime(elapsedTime);
    }

    private void RenderScene()
    {
        BeginRender();
        if (_initAnimationFinished)
        {
            _scene.ShowShadow = _showShadow;
            _scene.ShowDepthTest = _showDepthTest;
            _scene.UseDebugCam = _useDebugCamera;
            if (_showShadow)
            {
                //_scene.DrawShadow(_graphicPipeline);
            }
            //if (_useDebugCamera)
            {
                //_scene.DrawDebugDepth(_graphicPipeline);
            }

            Matrix4x4 matViewProj;
            if (_useDebugCamera)
            {
                ref readonly Matrix4x4 matProj = ref _debugCamera.FrustrumView.MatProj;
                _debugCamera.GetViewMatrix(out Matrix4x4 matView);
                matViewProj = matView * matProj;
            }
            else
            {
                ref readonly Matrix4x4 matProj = ref _gameCamera.FrustrumView.MatProj;
                _gameCamera.GetViewMatrix(out Matrix4x4 matView);
                matViewProj = matView * matProj;
            }
            _scene.Draw(_graphicPipeline, in matViewProj);
        }
    }
}
