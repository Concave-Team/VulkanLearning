using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace VulkanLearning
{
    public static class Helper
    {
        public static void VkCall(Result result)
        {
            if (result != Result.Success)
            {
                throw new Exception("Failed to create vkInstance. [" + result + "]");
            }
        }

        public unsafe static string BytePtrToString(byte* bytePointer)
        {
            return Marshal.PtrToStringAnsi((IntPtr)bytePointer);
        }
    }
}
