using MPSSELight.Protocol;
using System;
using System.Threading;

namespace TwoWireDevices.Tcs34725
{
    public class Tcs34725 : TwoWireBase
    {
        private const int TCS34725_ADDRESS = 0x29; // I2C address *

        // Command Bit
        private const int TCS34725_COMMAND_BIT = 0x80; // Command bit *

        // Address Register
        private const int TCS34725_ENABLE = 0x00; // Interrupt Enable register

        private const int TCS34725_ATIME = 0x01; // Integration time
        private const int TCS34725_WTIME = 0x03; // Wait time (if TCS34725_ENABLE_WEN is asserted)
        private const int TCS34725_AILTL = 0x04; // Clear channel lower interrupt threshold (lower byte)
        private const int TCS34725_AILTH = 0x05; // Clear channel lower interrupt threshold (higher byte)
        private const int TCS34725_AIHTL = 0x06; // Clear channel upper interrupt threshold (lower byte)
        private const int TCS34725_AIHTH = 0x07; // Clear channel upper interrupt threshold (higher byte)
        private const int TCS34725_PERS = 0x0C; // Persistence register - basic SW filtering mechanism for interrupts
        private const int TCS34725_CONFIG = 0x0D; // Configuration *
        private const int TCS34725_CONTROL = 0x0F; // Set the gain level for the sensor
        private const int TCS34725_ID = 0x12; // 0x44 = TCS34721/TCS34725, 0x4D = TCS34723/TCS34727
        private const int TCS34725_STATUS = 0x13; // Device status *
        private const int TCS34725_CDATAL = 0x14; // Clear channel data low byte
        private const int TCS34725_CDATAH = 0x15; // Clear channel data high byte
        private const int TCS34725_RDATAL = 0x16; // Red channel data low byte
        private const int TCS34725_RDATAH = 0x17; // Red channel data high byte
        private const int TCS34725_GDATAL = 0x18; // Green channel data low byte
        private const int TCS34725_GDATAH = 0x19; // Green channel data high byte
        private const int TCS34725_BDATAL = 0x1A; // Blue channel data low byte
        private const int TCS34725_BDATAH = 0x1B; // Blue channel data high byte

        //Enable Register (0x00) TCS34725_ENABLE
        private const int TCS34725_ENABLE_PON = 0x01; // Power on - Writing 1 activates the internal oscillator, 0 disables it

        private const int TCS34725_ENABLE_AEN = 0x02; // RGBC Enable - Writing 1 actives the ADC, 0 disables it
        private const int TCS34725_ENABLE_WEN = 0x08; // Wait Enable - Writing 1 activates the wait timer
        private const int TCS34725_ENABLE_AIEN = 0x10; // RGBC Interrupt Enable

        //Wait Time Register (0x03)
        private const int TCS34725_WTIME_2_4MS = 0xFF; // WLONG0 = 2.4ms   WLONG1 = 0.029s

        private const int TCS34725_WTIME_204MS = 0xAB; // WLONG0 = 204ms   WLONG1 = 2.45s
        private const int TCS34725_WTIME_614MS = 0x00; // WLONG0 = 614ms   WLONG1 = 7.4s

        private const int TCS34725_CONFIG_WLONG = 0x02; // Choose between short and long (12x) wait times via TCS34725_WTIME

        // Device Status (0x13) TCS34725_STATUS        private const int TCS34725_STATUS_AINT = 0x10; // RGBC Clean channel interrupt
        private const int TCS34725_STATUS_AVALID = 0x01; // Indicates that the RGBC channels have completed an integration cycle

        // TCS34725_PERS = 0x0C; // Persistence register - basic SW filtering mechanism for interrupts
        private const int TCS34725_PERS_NONE = 0b0000; // Every RGBC cycle generates an interrupt

        private const int TCS34725_PERS_1_CYCLE = 0b0001; // 1 clean channel value outside threshold range generates an interrupt
        private const int TCS34725_PERS_2_CYCLE = 0b0010; // 2 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_3_CYCLE = 0b0011; // 3 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_5_CYCLE = 0b0100; // 5 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_10_CYCLE = 0b0101; // 10 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_15_CYCLE = 0b0110; // 15 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_20_CYCLE = 0b0111; // 20 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_25_CYCLE = 0b1000; // 25 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_30_CYCLE = 0b1001; // 30 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_35_CYCLE = 0b1010; // 35 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_40_CYCLE = 0b1011; // 40 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_45_CYCLE = 0b1100; // 45 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_50_CYCLE = 0b1101; // 50 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_55_CYCLE = 0b1110; // 55 clean channel values outside threshold range generates an interrupt
        private const int TCS34725_PERS_60_CYCLE = 0b1111; // 60 clean channel values outside threshold range generates an interrupt
        private int _blueChannel;
        private int _clearChannel;
        private int _greenChannel;
        private int _redChannel;
        private Tcs34725Gain _tcs34725Gain;
        private bool _tcs34725Initialised;
        private Tcs34725IntegrationTime _tcs34725IntegrationTime;

        public Tcs34725(I2cBus i2c, Tcs34725IntegrationTime it, Tcs34725Gain gain)
            : this(i2c, TCS34725_ADDRESS, it, gain)
        {
        }

        public Tcs34725(I2cBus i2c, byte address, Tcs34725IntegrationTime it, Tcs34725Gain gain)
            : base(i2c, address)
        {
            _tcs34725Initialised = false;
            _tcs34725IntegrationTime = it;
            _tcs34725Gain = gain;
        }

        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }

        public void Enable()
        {
            WriteByte(TCS34725_ENABLE | TCS34725_COMMAND_BIT, TCS34725_ENABLE_PON);
            Thread.Sleep(3);
            WriteByte(TCS34725_ENABLE | TCS34725_COMMAND_BIT, TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            IntegrationTime(_tcs34725IntegrationTime);
        }

        private void IntegrationTime(Tcs34725IntegrationTime tcs34725IntegrationTime)
        {
            /* Set a Thread.Sleep for the integration time.
              This is only necessary in the case where enabling and then
              immediately trying to read values back. This is because setting
              AEN triggers an automatic integration, so if a read RGBC is
              performed too quickly, the data is not yet valid and all 0's are
              returned */
            switch (tcs34725IntegrationTime)
            {
                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_2_4MS:
                    Thread.Sleep(3);
                    break;

                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_24MS:
                    Thread.Sleep(24);
                    break;

                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_50MS:
                    Thread.Sleep(50);
                    break;

                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_101MS:
                    Thread.Sleep(101);
                    break;

                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_154MS:
                    Thread.Sleep(154);
                    break;

                case Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_700MS:
                    Thread.Sleep(700);
                    break;
            }
        }

        public void Disable()
        {
            /* Turn the device off to save power */
            var reg = ReadByte(TCS34725_ENABLE | TCS34725_COMMAND_BIT);
            WriteByte(TCS34725_ENABLE, (byte)(reg & ~(TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN)));
        }

        public bool Init()
        {
            /* Make sure we're actually connected */
            var id = ReadByte(TCS34725_ID | TCS34725_COMMAND_BIT);
            if (id != 0x44 && id != 0x10)
            {
                Console.WriteLine("Tcs Init Failed");
                return false;
            }

            _tcs34725Initialised = true;

            /* Set default integration time and gain */
            SetIntegrationTime(_tcs34725IntegrationTime);
            SetGain(_tcs34725Gain);

            /* Note: by default, the device is in power down mode on bootup */
            Enable();

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Red)}: {Red}, {nameof(Green)}: {Green}, {nameof(Blue)}: {Blue}";
        }

        /// <summary>
        /// </summary>
        /// <param name="tcs34725IntegrationTime"></param>
        public void SetIntegrationTime(Tcs34725IntegrationTime tcs34725IntegrationTime)
        {
            if (!_tcs34725Initialised)
                Init();

            /* Update the timing register */
            WriteByte(TCS34725_ATIME | TCS34725_COMMAND_BIT, (byte)tcs34725IntegrationTime);

            /* Update value placeholders */
            _tcs34725IntegrationTime = tcs34725IntegrationTime;
        }

        /*!
         *  @brief  Adjusts the gain on the TCS34725
         *  @param  gain
         *          Gain (sensitivity to light)
         */

        public void SetGain(Tcs34725Gain gain)
        {
            if (!_tcs34725Initialised)
                Init();

            /* Update the timing register */
            WriteByte(TCS34725_CONTROL | TCS34725_COMMAND_BIT, (byte)gain);

            /* Update value placeholders */
            _tcs34725Gain = gain;
        }

        public void GetData()
        {
            if (!_tcs34725Initialised)
                Init();

            var ccBytes = ReadBytes(TCS34725_CDATAL | TCS34725_COMMAND_BIT, 8);
            _clearChannel = (ccBytes[0] << 8) | ccBytes[1];
            _redChannel = (ccBytes[2] << 8) | ccBytes[3];
            _greenChannel = (ccBytes[4] << 8) | ccBytes[5];
            _blueChannel = (ccBytes[6] << 8) | ccBytes[7];

            ushort redColorSensor;
            ushort greenColorSensor;
            ushort blueColorSensor;

            if (_clearChannel > 0)
            {
                redColorSensor = (ushort)(_redChannel * 1.0 / (0.002 * _clearChannel));
                greenColorSensor = (ushort)(_greenChannel * 1.0 / (0.002 * _clearChannel));
                blueColorSensor = (ushort)(_blueChannel * 1.2 / (0.002 * _clearChannel));
            }
            else
            {
                redColorSensor = (ushort)(1.0 * (_redChannel >> 6));
                greenColorSensor = (ushort)(1.0 * (_greenChannel >> 6));
                blueColorSensor = (ushort)(1.0 * (_blueChannel >> 6));
            }

            Red = (byte)(redColorSensor < 255 ? redColorSensor : 0xFF);
            Green = (byte)(greenColorSensor < 255 ? greenColorSensor : 0xFF);
            Blue = (byte)(blueColorSensor < 255 ? blueColorSensor : 0xFF);

            /* Set a delay for the integration time */
            IntegrationTime(_tcs34725IntegrationTime);
        }
    }

    //Integration time settings for TCS34725

    // Gain settings for TCS34725
}