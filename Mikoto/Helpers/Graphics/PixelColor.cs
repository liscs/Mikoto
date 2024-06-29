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

        public readonly float Hue
        {
            get
            {
                int r = Red;
                int g = Green;
                int b = Blue;

                if (r == g && g == b)
                    return 0f;

                MinMaxRgb(out int min, out int max, r, g, b);

                float delta = max - min;
                float hue;

                if (r == max)
                    hue = (g - b) / delta;
                else if (g == max)
                    hue = (b - r) / delta + 2f;
                else
                    hue = (r - g) / delta + 4f;

                hue *= 60f;
                if (hue < 0f)
                    hue += 360f;

                return hue;
            }
        }

        private static void MinMaxRgb(out int min, out int max, int r, int g, int b)
        {
            if (r > g)
            {
                max = r;
                min = g;
            }
            else
            {
                max = g;
                min = r;
            }
            if (b > max)
            {
                max = b;
            }
            else if (b < min)
            {
                min = b;
            }
        }

    }


}
