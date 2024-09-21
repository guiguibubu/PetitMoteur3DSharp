using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;

namespace PetitMoteur3D
{
    internal class Program
    {
        private static IWindow _window = default!;
        private static DeviceD3D11 _deviceD3D11 = default!;
        static void Main(string[] args)
        {
            _window = WindowManager.Create();

            // Assign events.
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
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
            InitScene();
        }

        private static void OnUpdate(double obj)
        {
            System.Console.WriteLine("OnUpdate");
        }

        private static void OnRender(double obj)
        {
            System.Console.WriteLine("OnRender");
            OnRenderBefore(obj);
        }

        private static void OnFramebufferResize(Vector2D<int> newSize)
        {
            // If the window resizes, we need to be sure to update the swapchain's back buffers.
            _deviceD3D11.Resize(newSize);
        }

        private static void OnRenderBefore(double obj)
        {
            System.Console.WriteLine("OnRenderBefore");
            _deviceD3D11.Clear();
            _deviceD3D11.Swapchain.Present(0, 0);
        }

        private static void InitRendering()
        {
            _deviceD3D11 = new(_window);
        }

        private static void InitScene()
        {

        }
    }
}
