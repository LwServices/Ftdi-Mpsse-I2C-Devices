using System.Runtime.InteropServices;

namespace TwoWireDevices.Example.ConsoleColor
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        public uint ColorDWORD;

        public COLORREF(byte r, byte g, byte b)
        {
            ColorDWORD = (uint)r + (((uint)g) << 8) + (((uint)b) << 16);
        }
    }
}