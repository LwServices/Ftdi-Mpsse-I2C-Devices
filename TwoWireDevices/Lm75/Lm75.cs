using MPSSELight.Protocol;
using System;

namespace TwoWireDevices.Lm75
{
    /// <summary>
    ///     LM75 Temperature sensor
    /// </summary>
    public class Lm75 : TwoWireBase
    {
        private const int LM75_I2CDEFAULTADDR = 0x48; /**< Device default slave address */
        private const int LM75_BROADCASTADDR = 0; /**< Device broadcast slave address */

        /** REGISTER addresses. */
        private const int LM75_CONF = 0x01; /**< RAM reg - Configuration */
        private const int LM75_TEMP = 0x00; /**< RAM reg - Temperature */
        private const int LM75_TOS = 0x03; /**< RAM reg - Overtemperature shutdown threshold */
        private const int LM75_THYST = 0x02; /**< RAM reg - Hysteresis */

        /** CONFIGURATION bits masks. */
        private const int LM75_CONF_RES = 0x00; /**< Manufacturer reserved bits */
        private const int LM75_CONF_OSFQUE_1 = 0x00; /**< OS fault queue programming value = 1 */
        private const int LM75_CONF_OSFQUE_2 = 0x08; /**< OS fault queue programming value = 2 */
        private const int LM75_CONF_OSFQUE_4 = 0x10; /**< OS fault queue programming value = 4 */
        private const int LM75_CONF_OSFQUE_6 = 0x18; /**< OS fault queue programming value = 6 */
        private const int LM75_CONF_OSPOL_AL = 0x00; /**< OS polarity selection active LOW */
        private const int LM75_CONF_OSPOL_AH = 0x04; /**< OS polarity selection active HIGH */
        private const int LM75_CONF_OSOM_COMP = 0x00; /**< OS operation mode - comparator */
        private const int LM75_CONF_OSOM_INT = 0x02; /**< OS operation mode - interrupt */
        private const int LM75_CONF_DOM_NORMAL = 0x00; /**< Device operation mode - normal */
        private const int LM75_CONF_DOM_SHUTDOWN = 0x01; /**< Device operation mode - shutdown */

        /// <summary>
        ///     Constructor with custom device address
        /// </summary>
        /// <param name="twi"></param>
        /// <param name="deviceAddress"></param>
        public Lm75(I2cBus twi, byte deviceAddress) : base(twi, deviceAddress)
        {
            SetConfig(LM75_CONF_RES);
        }

        /// <summary>
        ///     Default Constructor
        /// </summary>
        /// <param name="twi"></param>
        public Lm75(I2cBus twi) : this(twi, LM75_I2CDEFAULTADDR)
        {
        }

        /// <summary>
        ///     Current Temperature
        /// </summary>
        public double GetTemperature()
        {
            var ret = ReadBytes(LM75_TEMP, 2);
            return (((ret[0] << 8) | ret[1]) >> 5) * 0.125;
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public byte GetConfig()
        {
            return ReadByte(LM75_CONF);
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public void SetConfig(byte value)
        {
            WriteByte(LM75_CONF, value);
        }

        /// <summary>
        ///     Tos
        /// </summary>
        public double GetTos()
        {
            var ret = ReadBytes(LM75_TOS, 2);
            return (((ret[0] << 8) | ret[1]) >> 7) * 0.5;
        }

        /// <summary>
        ///     Tos
        /// </summary>
        public void SetTos(double value)
        {
            var (msb, lsb) = ConvertToByteTuple(value);

            WriteBytes(LM75_TOS, new[] { msb, lsb });
        }

        /// <summary>
        ///     Thyst Temperature
        /// </summary>
        public double GetThyst()
        {
            var ret = ReadBytes(LM75_THYST, 2);
            return (((ret[0] << 8) | ret[1]) >> 7) * 0.5;
        }

        /// <summary>
        ///     Thyst Temperature
        /// </summary>
        public void SetThyst(double value)
        {
            var (msb, lsb) = ConvertToByteTuple(value);
            WriteBytes(LM75_THYST, new[] { msb, lsb });
        }

        private static (byte msb, byte lsb) ConvertToByteTuple(double temperature)
        {
            int msb, lsb;

            if (temperature >= 0)
            {
                lsb = temperature % 1 >= 0.5 ? 0x80 : 0x00;
                msb = (int)temperature & 0xFF;
            }
            else
            {
                var temp = (int)Math.Floor(temperature * 2) & 0x1FF;
                lsb = (temp & 0x01) == 1 ? 0x80 : 0x00;
                msb = (temp / 2) & 0xFF;
            }

            return ((byte)msb, (byte)lsb);
        }
    }
}