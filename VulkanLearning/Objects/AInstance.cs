using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using VulkanLearning.Extensions;
using VulkanLearning.Structs;
using System.Runtime.InteropServices;

namespace VulkanLearning.Objects
{
    public class AInstance
    {
        public Instance vkInstance;
        public ApplicationInfo AppInfo;
        public IWindow AppWindow;
        public ValidationLayerInfo validationLayers;
        public List<string>? Extensions;

        private static Vk vk { get
            {
                return Vk.GetApi();
            } }

        private unsafe static bool CheckValidationLayerSupport(ValidationLayerInfo VLInfo)
        {
            uint LayerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref LayerCount, null);

            LayerProperties[] layerProps = new LayerProperties[LayerCount];
            fixed(LayerProperties* pLayerProps = layerProps)
                vk.EnumerateInstanceLayerProperties(ref LayerCount, pLayerProps);

            var aLayerNames = layerProps.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();
            return VLInfo.ValidationLayers.All(aLayerNames.Contains);
        }

        public unsafe static AInstance CreateInstance(IWindow window, ValidationLayerInfo vlInfo, string AppName, string EngineName, uint ApiVersion, uint EngineVersion, uint AppVersion, List<string>? extensions = null)
        {
            if (vlInfo.EnableValidationLayers && !CheckValidationLayerSupport(vlInfo))
                throw new Exception("Validation Layers requested, but not available!");
            AInstance instance = new AInstance();
            instance.validationLayers = vlInfo;
            instance.Extensions = extensions;
            instance.AppWindow = window;

            instance.AppInfo.PApplicationName = AppName.ToBytePointer();
            instance.AppInfo.PEngineName = EngineName.ToBytePointer();
            instance.AppInfo.SType = StructureType.ApplicationInfo;
            instance.AppInfo.ApiVersion = ApiVersion;
            instance.AppInfo.ApplicationVersion = AppVersion;
            instance.AppInfo.EngineVersion = EngineVersion;
            instance.AppInfo.PNext = null;

            fixed (ApplicationInfo* pAppInfo = &instance.AppInfo)
            {
                InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo(pApplicationInfo: pAppInfo);

                var glfw = Glfw.GetApi();

                uint extCount = 0;

                byte** exts = null;
                exts = glfw.GetRequiredInstanceExtensions(out extCount);

                var newExtensions = stackalloc byte*[(int)(extCount + extensions?.Count)];

                for (int i = 0; i < extCount; i++)
                {
                    newExtensions[i] = exts[i];
                }

                for (int i = 0; i < extensions?.Count; i++)
                {
                    newExtensions[i+extCount] = extensions[i].ToBytePointer();
                }

                instanceCreateInfo.EnabledExtensionCount = extCount;
                instanceCreateInfo.PpEnabledExtensionNames = newExtensions;

                if(vlInfo.EnableValidationLayers)
                {
                    instanceCreateInfo.EnabledLayerCount = (uint)vlInfo.ValidationLayers.Count;

                    // construct byte** from List<LayerProperties>

                    byte*[] layerNamesB = new byte*[vlInfo.ValidationLayers.Count];

                    for(int i = 0; i < layerNamesB.Length; i++)
                        layerNamesB[i] = vlInfo.ValidationLayers[i].ToBytePointer();

                    fixed (byte** LayerArray = layerNamesB)
                        instanceCreateInfo.PpEnabledLayerNames = LayerArray;
                }
                else
                    instanceCreateInfo.EnabledLayerCount = 0;

                fixed(Instance* pInstance = &instance.vkInstance)
                    Helper.VkCall(vk.CreateInstance(in instanceCreateInfo, null, pInstance));

                uint ExtCount = 0;
                Helper.VkCall(vk.EnumerateInstanceExtensionProperties((byte*)null, ref ExtCount, null));

                ExtensionProperties[] extProps = new ExtensionProperties[ExtCount];

                fixed (ExtensionProperties* pExtProps = extProps)
                {
                    Helper.VkCall(vk.EnumerateInstanceExtensionProperties((byte*)null, ref ExtCount, pExtProps));

                    Console.WriteLine("Available Extensions:");
                    foreach (var ext in extProps)
                    {
                        Console.WriteLine($"\t{Helper.BytePtrToString(ext.ExtensionName)}");
                    }
                }
            }

            return instance;
        }

        public unsafe void Destroy()
        {
            var vk = Vk.GetApi();
            vk.DestroyInstance(vkInstance, null);
        }
    }
}
