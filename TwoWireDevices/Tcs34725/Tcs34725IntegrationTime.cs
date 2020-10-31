namespace TwoWireDevices.Tcs34725
{
    public enum Tcs34725IntegrationTime
    {
        TCS34725_INTEGRATIONTIME_2_4MS = 0xFF, //  2.4ms - 1 cycle    - Max Count: 1024
        TCS34725_INTEGRATIONTIME_24MS = 0xF6, //  24ms  - 10 cycles  - Max Count: 10240
        TCS34725_INTEGRATIONTIME_50MS = 0xEB, //  50ms  - 20 cycles  - Max Count: 20480
        TCS34725_INTEGRATIONTIME_101MS = 0xD5, //  101ms - 42 cycles  - Max Count: 43008
        TCS34725_INTEGRATIONTIME_154MS = 0xC0, //  154ms - 64 cycles  - Max Count: 65535
        TCS34725_INTEGRATIONTIME_700MS = 0x00 //  700ms - 256 cycles - Max Count: 65535
    }
}