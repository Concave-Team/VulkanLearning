using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Silk.NET.GLFW;
using VulkanLearning.Extensions;
using VulkanLearning.Structs;
using System.Runtime.InteropServices;
using Silk.NET.Core;

namespace VulkanLearning.Objects
{
    public struct QueueFamilyIndices
    {
        public UInt32? graphicsFamily;
        public UInt32? presentFamily;

        public bool IsComplete()
        {
            return graphicsFamily.HasValue && presentFamily.HasValue;
        }
    }

    /// <summary>
    /// Contains the Vulkan Physical Device and Logical Device.
    /// </summary>
    public class APhysicalDevice
    {
        public PhysicalDevice vkPhysicalDevice;
        public AInstance vkInstance;
        public Device vkDevice;
        public AWindowSurface WindowSurface;
        public Queue GraphicsQueue;
        public Queue PresentQueue;

        private static Vk vk
        {
            get
            {
                return Vk.GetApi();
            }
        }

        private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice dev)
        {
            uint extCount = 0;
            vk.EnumerateDeviceExtensionProperties(dev, (string)null, ref extCount, null);

            ExtensionProperties[] availableExts = new ExtensionProperties[extCount];

            fixed(ExtensionProperties* pAvailableExts = availableExts)
                vk.EnumerateDeviceExtensionProperties(dev, (string)null, ref extCount, pAvailableExts);

            List<string> devExtensions = vkInstance.Extensions;

            foreach(var ext in availableExts)
            {

            }
        }

        private unsafe bool IsDeviceSuitable(PhysicalDevice dev)
        {
            PhysicalDeviceProperties devProps;
            vk.GetPhysicalDeviceProperties(dev, out devProps);

            PhysicalDeviceFeatures devFeats;
            vk.GetPhysicalDeviceFeatures(dev, out devFeats);

            QueueFamilyIndices devIndices = this.FindQueueFamilies(dev);

            return devIndices.IsComplete();
        }

        public unsafe void PickDevice()
        {
            uint devCount = 0;

            Helper.VkCall(vk.EnumeratePhysicalDevices(vkInstance.vkInstance, ref devCount, null));

            if(devCount == 0)
            {
                throw new Exception("Could not find any GPUs that have Vulkan support.");
            }

            PhysicalDevice[] physDevs = new PhysicalDevice[devCount];

            fixed(PhysicalDevice* pPhysDevs = physDevs)
                Helper.VkCall(vk.EnumeratePhysicalDevices(vkInstance.vkInstance, ref devCount, pPhysDevs));

            foreach(var dev in physDevs)
            {
                if(IsDeviceSuitable(dev))
                {
                    vkPhysicalDevice = dev;
                    break;
                }
            }

            if (vkPhysicalDevice.Handle == 0)
                throw new Exception("Could not find a suitable GPU.");

            // Create Logical Device:

            QueueFamilyIndices indices = this.FindQueueFamilies(vkPhysicalDevice);

            float queuePriority = 1.0f;

            var uniqueQueueFamilies = indices.graphicsFamily.Value == indices.presentFamily.Value
                ? new[] { indices.graphicsFamily.Value }
                : new[] { indices.graphicsFamily.Value, indices.presentFamily.Value };

            List<DeviceQueueCreateInfo> queueCreateInfos = new List<DeviceQueueCreateInfo>();

            for(int a = 0; a < uniqueQueueFamilies.Length; a++)
            {
                queueCreateInfos.Add(new DeviceQueueCreateInfo(queueFamilyIndex: indices.graphicsFamily.Value, queueCount: 1, pQueuePriorities: &queuePriority));
            }

            //DeviceQueueCreateInfo dQCreateInfo = new DeviceQueueCreateInfo(queueFamilyIndex: indices.graphicsFamily.Value, queueCount:1, pQueuePriorities:&queuePriority);
            PhysicalDeviceFeatures physDevFeatures = new PhysicalDeviceFeatures();
            var qInfos = queueCreateInfos.ToArray();
            DeviceCreateInfo devCreateInfo;
            fixed (DeviceQueueCreateInfo* pQCreateInfo = qInfos)
                devCreateInfo = new DeviceCreateInfo(pQueueCreateInfos: pQCreateInfo, queueCreateInfoCount: 1, pEnabledFeatures: &physDevFeatures, enabledExtensionCount: 0);
            if (vkInstance.validationLayers.EnableValidationLayers)
            {
                devCreateInfo.EnabledLayerCount = (uint)vkInstance.validationLayers.ValidationLayers.Count;
                byte*[] layerNamesB = new byte*[vkInstance.validationLayers.ValidationLayers.Count];

                for (int i = 0; i < layerNamesB.Length; i++)
                    layerNamesB[i] = vkInstance.validationLayers.ValidationLayers[i].ToBytePointer();

                fixed (byte** LayerArray = layerNamesB)
                    devCreateInfo.PpEnabledLayerNames = LayerArray;
            }
            else
                devCreateInfo.EnabledLayerCount = 0;
            fixed(Device* pVkDevice = &vkDevice)
                Helper.VkCall(vk.CreateDevice(vkPhysicalDevice, in devCreateInfo, null, pVkDevice));

            fixed(Queue* pQueue = &GraphicsQueue)
                vk.GetDeviceQueue(vkDevice, indices.graphicsFamily.Value, 0, pQueue);
            fixed (Queue* pQueue = &PresentQueue)
                vk.GetDeviceQueue(vkDevice, indices.presentFamily.Value, 0, pQueue);
        }

        public unsafe void Destroy()
        {
            vk.DestroyDevice(vkDevice, null);
        }

        public unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice dev)
        {
            QueueFamilyIndices indices = new QueueFamilyIndices();

            uint qFamilyCount = 0;
            vk.GetPhysicalDeviceQueueFamilyProperties(dev, &qFamilyCount, null);

            QueueFamilyProperties[] qFamilyProperties = new QueueFamilyProperties[qFamilyCount];
            fixed(QueueFamilyProperties* pQueueFamilyProperties = qFamilyProperties)
                vk.GetPhysicalDeviceQueueFamilyProperties(dev, &qFamilyCount, pQueueFamilyProperties);

            int i = 0;

            foreach (var qFamProp in qFamilyProperties)
            {
                if(Convert.ToBoolean(qFamProp.QueueFlags & QueueFlags.GraphicsBit))
                {
                    indices.graphicsFamily = (uint?)i;
                }

                Bool32 presentSupport = false;
                Helper.VkCall(WindowSurface.Surface.GetPhysicalDeviceSurfaceSupport(dev, (uint)i, WindowSurface.kSurface, out presentSupport));

                if (presentSupport)
                {
                    indices.presentFamily = (uint?)i;
                }

                if (indices.IsComplete())
                    break;
                i++;
            }

            return indices;
        }

        public static APhysicalDevice GetPhysicalDevice(AInstance instance, AWindowSurface surface)
        {
            APhysicalDevice physDev = new APhysicalDevice();

            physDev.vkInstance = instance;
            physDev.WindowSurface = surface;
            physDev.PickDevice();

            return physDev;
        }

        public APhysicalDevice() { }
    }
}
