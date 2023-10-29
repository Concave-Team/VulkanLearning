using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan.Extensions.KHR;

namespace VulkanLearning.Objects
{
    public class AWindowSurface
    {
        public AInstance Instance;
        public KhrSurface Surface;
        public SurfaceKHR kSurface;
        public IWindow _Window;

        private static Vk vk
        {
            get
            {
                return Vk.GetApi();
            }
        }

        public unsafe static AWindowSurface CreateSurface(IWindow window, AInstance instance)
        {
            AWindowSurface surface = new AWindowSurface();

            surface._Window = window;
            surface.Instance = instance;
            var glfw = Glfw.GetApi();

            surface.kSurface = surface._Window.VkSurface!.Create<AllocationCallbacks>(instance.vkInstance.ToHandle(), null).ToSurface();

            if (!vk.TryGetInstanceExtension<KhrSurface>(surface.Instance.vkInstance, out surface.Surface))
                throw new NotSupportedException("KHR_surface extension not found.");

            return surface;
        }

        public unsafe void Destroy()
        {
            Surface.DestroySurface(Instance.vkInstance, kSurface, null);
        }
    }
}
