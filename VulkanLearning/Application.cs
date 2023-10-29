using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Windowing;
using Silk.NET.Vulkan;
using VulkanLearning.Objects;
using VulkanLearning.Structs;
using VulkanLearning.Extensions;

namespace VulkanLearning
{
    public class Application
    {
        public unsafe void Run()
        {
            Console.WriteLine("Hello from this app!");

            var window = Window.Create(WindowOptions.DefaultVulkan);
            //window.Initialize();

            ValidationLayerInfo vlInfo = new ValidationLayerInfo();
            vlInfo.ValidationLayers.Add("VK_LAYER_KHRONOS_validation");
            //vlInfo.ValidationLayers.Add("VK_LAYER_LUNARG_api_dump");        
            APhysicalDevice physicalDevice = null;
            AWindowSurface surface = null;
            AInstance instance = null;

            window.Load += () => {
                instance = AInstance.CreateInstance(window, vlInfo, "VulkanLearning", "No Engine", Vk.Version11, Vk.MakeVersion(1, 0, 0), Vk.MakeVersion(1, 0, 0), new List<string>
                {
                    // Extensions:
                    "VK_KHR_swapchain"
                });

                surface = AWindowSurface.CreateSurface(window, instance);

                physicalDevice = APhysicalDevice.GetPhysicalDevice(instance, surface);
            };

            window.Update += dt =>
            {
                
            };

            window.Render += dt =>
            {

            };

            window.Run();
            physicalDevice!.Destroy();
            surface!.Destroy();
            instance!.Destroy();          
                 
        }

        public Application() { }
    }
}
