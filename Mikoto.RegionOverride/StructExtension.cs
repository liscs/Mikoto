using System.Runtime.InteropServices;

namespace Mikoto.RegionOverride
{
    internal static class StructExtension
    {

        internal static byte[] ToByteArray(this ValueType structObj)
        {
            var size = Marshal.SizeOf(structObj);
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, true);
                var bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}