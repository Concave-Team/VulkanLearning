using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace VulkanLearning.Structs
{
    /// <summary>
    /// Specifies data about validation layers.
    /// </summary>
    public class ValidationLayerInfo
    {
        // List of validation layers to enable.
        public List<string> ValidationLayers = new List<string>();
        // Whether to enable validation layers at all, true by default.
        public bool EnableValidationLayers = true;
    }
}
