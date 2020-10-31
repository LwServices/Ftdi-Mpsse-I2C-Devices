using MPSSELight.Protocol;

namespace TwoWireDevices.Mcp4725
{
    /// <summary>
    ///     https://github.com/adafruit/Adafruit_MCP4725
    ///     https://learn.sparkfun.com/tutorials/mcp4725-digital-to-analog-converter-hookup-guide?_ga=2.228778268.1223093885.1543822927-1562082230.1542101095
    /// </summary>
    public class Mcp4725 : TwoWireBase
    {
        private const int MCP4726_I2CDEFAULTADDRESS = 0x62;

        private const int MCP4726_CMD_WRITEDAC = 0x40; // Writes data to the DAC
        private const int MCP4726_CMD_WRITEDACEEPROM = 0x60; // Writes data to the DAC and the EEPROM (persisting the assigned value after reset)

        /// <inheritdoc />
        public Mcp4725(I2cBus twi, byte deviceAddress) : base(twi, deviceAddress)
        {
        }

        public Mcp4725(I2cBus twi) : base(twi, MCP4726_I2CDEFAULTADDRESS)
        {
        }

        /// <summary>
        ///     Sets the output voltage to a fraction of source vref.  (Value can be 0..4095)
        /// </summary>
        /// <param name="output"></param>
        /// <param name="writeEEPROM"></param>
        public void SetVoltage(ushort output, bool writeEEPROM)
        {
            var msb = (byte) (output >> 4); // Upper data bits          (D11.D10.D9.D8.D7.D6.D5.D4)
            var lsb = (byte) ((output << 4) & 0xFF); // Lower data bits          (D3.D2.D1.D0.x.x.x.x)
            if (writeEEPROM)
                WriteBytes(MCP4726_CMD_WRITEDACEEPROM, new[] {msb, lsb});
            else
                WriteBytes(MCP4726_CMD_WRITEDAC, new[] {msb, lsb});
        }
    }
}