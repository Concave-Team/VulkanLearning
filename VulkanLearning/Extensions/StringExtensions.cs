using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;

namespace VulkanLearning.Extensions
{

    public static class StringExtensions
    {
        public static unsafe sbyte* ToSBytePointer(this string str)
        {
            if (str == null)
            {
                return null;
            }

            byte[] byteArray = new byte[str.Length + 1]; // +1 for the null terminator
            for (int i = 0; i < str.Length; i++)
            {
                byteArray[i] = (byte)str[i];
            }
            byteArray[str.Length] = 0; // Null-terminate the byte array

            fixed (byte* byteArrayPtr = byteArray)
            {
                return (sbyte*)byteArrayPtr;
            }
        }

        public unsafe static byte* ToBytePointer(this string str)
        {
            if (str == null)
            {
                return null;
            }

            byte[] byteArray = new byte[str.Length + 1]; // +1 for the null terminator
            for (int i = 0; i < str.Length; i++)
            {
                byteArray[i] = (byte)str[i];
            }
            byteArray[str.Length] = 0; // Null-terminate the byte array

            fixed (byte* byteArrayPtr = byteArray)
            {
                return byteArrayPtr;
            }
        }
    }
}
