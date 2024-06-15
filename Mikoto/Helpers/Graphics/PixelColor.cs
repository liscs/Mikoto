using System.Runtime.InteropServices;

namespace Mikoto.Helpers
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)] public uint ColorBGRA;
        // 8 bit components
        [FieldOffset(0)] public byte Blue;
        [FieldOffset(1)] public byte Green;
        [FieldOffset(2)] public byte Red;
        [FieldOffset(3)] public byte Alpha;
    }


}
