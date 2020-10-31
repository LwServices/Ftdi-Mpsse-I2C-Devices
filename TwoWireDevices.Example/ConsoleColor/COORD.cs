using System.Runtime.InteropServices;

namespace TwoWireDevices.Example.ConsoleColor
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;
    }
}