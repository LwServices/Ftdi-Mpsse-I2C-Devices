using MPSSELight.Protocol;

namespace TwoWireDevices.Epprom
{
    /// <summary>
    ///     https://breadboardtronics.wordpress.com/2013/08/27/at24c32-eeprom-and-arduino/
    /// </summary>
    public class AT24C32 : TwoWireBase
    {
        private const int AT24C32_I2CDEFAULTADDR = 0x50; /**< Device default slave address */

        /// <summary>
        ///     Default Constructor
        /// </summary>
        /// <param name="twi"></param>
        public AT24C32(I2cBus twi) : this(twi, AT24C32_I2CDEFAULTADDR)
        {
        }

        /// <summary>
        ///     Constructor with custom device address
        /// </summary>
        /// <param name="twi"></param>
        /// <param name="deviceAddress"></param>
        public AT24C32(I2cBus twi, byte deviceAddress) : base(twi, deviceAddress)
        {
        }
    }
}