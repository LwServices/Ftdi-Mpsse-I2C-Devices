using MPSSELight.Protocol;
using System;
using System.Threading;

//  Original "Code Logic" from:
//      @file     Adafruit_ADS1015.cpp
//      @author   K.Townsend (Adafruit Industries)
//      @license  BSD (see license.txt)
//      Driver for the ADS1015/ADS1115 ADC
//      https://github.com/adafruit/Adafruit_ADS1X15/blob/master/Adafruit_ADS1015.cpp

namespace TwoWireDevices.Ads1115
{
    public class ADS1015 : TwoWireBase
    {
        /*=========================================================================*/

        public enum AdsGain : ushort
        {
            GAIN_TWOTHIRDS = ADS1015_REG_CONFIG_PGA_6_144V,
            GAIN_ONE = ADS1015_REG_CONFIG_PGA_4_096V,
            GAIN_TWO = ADS1015_REG_CONFIG_PGA_2_048V,
            GAIN_FOUR = ADS1015_REG_CONFIG_PGA_1_024V,
            GAIN_EIGHT = ADS1015_REG_CONFIG_PGA_0_512V,
            GAIN_SIXTEEN = ADS1015_REG_CONFIG_PGA_0_256V
        }

        /*=========================================================================
        I2C ADDRESS/BITS
        -----------------------------------------------------------------------*/
        protected const byte ADS1015_ADDRESS = 0x48; // 1001 000 =ADDR = GND;
        /*=========================================================================*/

        /*=========================================================================
            CONVERSION DELAY =in mS;
            -----------------------------------------------------------------------*/
        protected const byte ADS1015_CONVERSIONDELAY = 1;

        protected const byte ADS1115_CONVERSIONDELAY = 8;
        /*=========================================================================*/

        /*=========================================================================
            POINTER REGISTER
            -----------------------------------------------------------------------*/
        protected const byte ADS1015_REG_POINTER_MASK = 0x03;
        protected const byte ADS1015_REG_POINTER_CONVERT = 0x00;
        protected const byte ADS1015_REG_POINTER_CONFIG = 0x01;
        protected const byte ADS1015_REG_POINTER_LOWTHRESH = 0x02;

        protected const byte ADS1015_REG_POINTER_HITHRESH = 0x03;
        /*=========================================================================*/

        /*=========================================================================
            CONFIG REGISTER
            -----------------------------------------------------------------------*/
        protected const ushort ADS1015_REG_CONFIG_OS_MASK = 0x8000;
        protected const ushort ADS1015_REG_CONFIG_OS_SINGLE = 0x8000; // Write: Set to start a single-conversion
        protected const ushort ADS1015_REG_CONFIG_OS_BUSY = 0x0000; // Read: Bit = 0 when conversion is in progress
        protected const ushort ADS1015_REG_CONFIG_OS_NOTBUSY = 0x8000; // Read: Bit = 1 when device is not performing a conversion

        protected const ushort ADS1015_REG_CONFIG_MUX_MASK = 0x7000;
        protected const ushort ADS1015_REG_CONFIG_MUX_DIFF_0_1 = 0x0000; // Differential P = AIN0, N = AIN1 =default;
        protected const ushort ADS1015_REG_CONFIG_MUX_DIFF_0_3 = 0x1000; // Differential P = AIN0, N = AIN3
        protected const ushort ADS1015_REG_CONFIG_MUX_DIFF_1_3 = 0x2000; // Differential P = AIN1, N = AIN3
        protected const ushort ADS1015_REG_CONFIG_MUX_DIFF_2_3 = 0x3000; // Differential P = AIN2, N = AIN3
        protected const ushort ADS1015_REG_CONFIG_MUX_SINGLE_0 = 0x4000; // Single-ended AIN0
        protected const ushort ADS1015_REG_CONFIG_MUX_SINGLE_1 = 0x5000; // Single-ended AIN1
        protected const ushort ADS1015_REG_CONFIG_MUX_SINGLE_2 = 0x6000; // Single-ended AIN2
        protected const ushort ADS1015_REG_CONFIG_MUX_SINGLE_3 = 0x7000; // Single-ended AIN3

        protected const ushort ADS1015_REG_CONFIG_PGA_MASK = 0x0E00;
        protected const ushort ADS1015_REG_CONFIG_PGA_6_144V = 0x0000; // +/-6.144V range = Gain 2/3
        protected const ushort ADS1015_REG_CONFIG_PGA_4_096V = 0x0200; // +/-4.096V range = Gain 1
        protected const ushort ADS1015_REG_CONFIG_PGA_2_048V = 0x0400; // +/-2.048V range = Gain 2 =default;
        protected const ushort ADS1015_REG_CONFIG_PGA_1_024V = 0x0600; // +/-1.024V range = Gain 4
        protected const ushort ADS1015_REG_CONFIG_PGA_0_512V = 0x0800; // +/-0.512V range = Gain 8
        protected const ushort ADS1015_REG_CONFIG_PGA_0_256V = 0x0A00; // +/-0.256V range = Gain 16

        protected const ushort ADS1015_REG_CONFIG_MODE_MASK = 0x0100;
        protected const ushort ADS1015_REG_CONFIG_MODE_CONTIN = 0x0000; // Continuous conversion mode
        protected const ushort ADS1015_REG_CONFIG_MODE_SINGLE = 0x0100; // Power-down single-shot mode =default;

        protected const ushort ADS1015_REG_CONFIG_DR_MASK = 0x00E0;
        protected const ushort ADS1015_REG_CONFIG_DR_128SPS = 0x0000; // 128 samples per second
        protected const ushort ADS1015_REG_CONFIG_DR_250SPS = 0x0020; // 250 samples per second
        protected const ushort ADS1015_REG_CONFIG_DR_490SPS = 0x0040; // 490 samples per second
        protected const ushort ADS1015_REG_CONFIG_DR_920SPS = 0x0060; // 920 samples per second
        protected const ushort ADS1015_REG_CONFIG_DR_1600SPS = 0x0080; // 1600 samples per second =default;
        protected const ushort ADS1015_REG_CONFIG_DR_2400SPS = 0x00A0; // 2400 samples per second
        protected const ushort ADS1015_REG_CONFIG_DR_3300SPS = 0x00C0; // 3300 samples per second

        protected const ushort ADS1015_REG_CONFIG_CMODE_MASK = 0x0010;
        protected const ushort ADS1015_REG_CONFIG_CMODE_TRAD = 0x0000; // Traditional comparator with hysteresis =default;
        protected const ushort ADS1015_REG_CONFIG_CMODE_WINDOW = 0x0010; // Window comparator

        protected const ushort ADS1015_REG_CONFIG_CPOL_MASK = 0x0008;
        protected const ushort ADS1015_REG_CONFIG_CPOL_ACTVLOW = 0x0000; // ALERT/RDY pin is low when active =default;
        protected const ushort ADS1015_REG_CONFIG_CPOL_ACTVHI = 0x0008; // ALERT/RDY pin is high when active

        protected const ushort ADS1015_REG_CONFIG_CLAT_MASK = 0x0004; // Determines if ALERT/RDY pin latches once asserted
        protected const ushort ADS1015_REG_CONFIG_CLAT_NONLAT = 0x0000; // Non-latching comparator =default;
        protected const ushort ADS1015_REG_CONFIG_CLAT_LATCH = 0x0004; // Latching comparator

        protected const ushort ADS1015_REG_CONFIG_CQUE_MASK = 0x0003;
        protected const ushort ADS1015_REG_CONFIG_CQUE_1CONV = 0x0000; // Assert ALERT/RDY after one conversions
        protected const ushort ADS1015_REG_CONFIG_CQUE_2CONV = 0x0001; // Assert ALERT/RDY after two conversions
        protected const ushort ADS1015_REG_CONFIG_CQUE_4CONV = 0x0002; // Assert ALERT/RDY after four conversions
        protected const ushort ADS1015_REG_CONFIG_CQUE_NONE = 0x0003; // Disable the comparator and put ALERT/RDY in high state =default;
        protected byte BitShift;
        protected byte ConversionDelay;
        protected AdsGain Gain;

        /**************************************************************************/
        /*!
            @brief  Instantiates a new ADS1015 class w/appropriate properties
        */
        /**************************************************************************/

        public ADS1015(I2cBus i2c)
            : this(i2c, ADS1015_ADDRESS)
        {
        }

        public ADS1015(I2cBus i2c, byte address)
            : base(i2c, address)
        {
            ConversionDelay = ADS1015_CONVERSIONDELAY;
            BitShift = 4;
            Gain = AdsGain.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }

        public double Vref
        {
            get
            {
                switch (Gain)
                {
                    case AdsGain.GAIN_TWOTHIRDS:
                        return 6.144;

                    case AdsGain.GAIN_ONE:
                        return 4.096;

                    case AdsGain.GAIN_TWO:
                        return 2.048;

                    case AdsGain.GAIN_FOUR:
                        return 1.024;

                    case AdsGain.GAIN_EIGHT:
                        return 0.512;

                    case AdsGain.GAIN_SIXTEEN:
                        return 0.256;
                }

                return 0;
            }
        }

        /**************************************************************************/
        /*!
            @brief  Gets a single-ended ADC reading from the specified channel
        */
        /**************************************************************************/

        public short ReadAdcSingleEnded(byte channel)
        {
            if (channel > 3) return 0;

            // Start with default values
            ushort config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (ushort)Gain;

            // Set single-ended input channel
            switch (channel)
            {
                case 0:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_0;
                    break;

                case 1:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_1;
                    break;

                case 2:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_2;
                    break;

                case 3:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_3;
                    break;
            }

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            //this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);
            WriteBytes(ADS1015_REG_POINTER_CONFIG, new[] { (byte)(config >> 8), (byte)(config & 0xFF) });

            // Wait for the conversion to complete
            SleepMilliSeconds(ConversionDelay);

            // Read the conversion results
            //var u16 = this.ReadRegister16Bits(ADS1015_REG_POINTER_CONVERT);
            var bytes = ReadBytes(ADS1015_REG_POINTER_CONVERT, 2);
            var u16 = (ushort)((bytes[0] << 8) | bytes[1]);
            //Ones' complement
            var signed = u16 > 0x7fff;
            if (signed) u16 = Convert.ToUInt16(~u16 & 0xffff);

            // Shift 12-bit results right 4 bits for the ADS1015
            var s16 = Convert.ToInt16(u16 >> BitShift);

            //ptp - fixed the signed bit - one's complement
            return Convert.ToInt16((signed ? -1 : 1) * s16);
        }

        /**************************************************************************/
        /*!
            @brief  Reads the conversion results, measuring the voltage
                    difference between the P (AIN0) and N (AIN1) input.  Generates
                    a signed value since the difference can be either
                    positive or negative.
        */
        /**************************************************************************/

        protected short ReadAdcDifferential_0_1()
        {
            // Start with default values
            ushort config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (ushort)Gain;

            // Set channels
            config |= ADS1015_REG_CONFIG_MUX_DIFF_0_1; // AIN0 = P, AIN1 = N

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            //this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);
            WriteBytes(ADS1015_REG_POINTER_CONFIG, new[] { (byte)(config >> 8), (byte)(config & 0xFF) });

            // Wait for the conversion to complete
            SleepMilliSeconds(ConversionDelay);

            // Read the conversion results
            var bytes = ReadBytes(ADS1015_REG_POINTER_CONVERT, 2);
            var u16 = (ushort)((bytes[0] << 8) | bytes[1]);
            var res = Convert.ToUInt16(u16 >> BitShift);
            if (BitShift == 0) return (short)res;

            // Shift 12-bit results right 4 bits for the ADS1015,
            // making sure we keep the sign bit intact
            if (res > 0x07FF) res |= 0xF000;
            return (short)res;
        }

        /**************************************************************************/
        /*!
            @brief  Reads the conversion results, measuring the voltage
                    difference between the P (AIN2) and N (AIN3) input.  Generates
                    a signed value since the difference can be either
                    positive or negative.
        */
        /**************************************************************************/

        protected short readADC_Differential_2_3()
        {
            // Start with default values
            ushort config = ADS1015_REG_CONFIG_CQUE_NONE | // Disable the comparator (default val)
                            ADS1015_REG_CONFIG_CLAT_NONLAT | // Non-latching (default val)
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_SINGLE; // Single-shot mode (default)

            // Set PGA/voltage range
            config |= (ushort)Gain;

            // Set channels
            config |= ADS1015_REG_CONFIG_MUX_DIFF_2_3; // AIN2 = P, AIN3 = N

            // Set 'start single-conversion' bit
            config |= ADS1015_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            //this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);
            WriteBytes(ADS1015_REG_POINTER_CONFIG, new[] { (byte)(config >> 8), (byte)(config & 0xFF) });

            // Wait for the conversion to complete
            SleepMilliSeconds(ConversionDelay);

            // Read the conversion results
            var bytes = ReadBytes(ADS1015_REG_POINTER_CONVERT, 2);
            var u16 = (ushort)((bytes[0] << 8) | bytes[1]);
            var res = Convert.ToUInt16(u16 >> BitShift);
            if (BitShift == 0) return (short)res;

            // Shift 12-bit results right 4 bits for the ADS1015,
            // making sure we keep the sign bit intact
            if (res > 0x07FF) res |= 0xF000;
            return (short)res;
        }

        /**************************************************************************/
        /*!
            @brief  Sets up the comparator to operate in basic mode, causing the
                    ALERT/RDY pin to assert (go from high to low) when the ADC
                    value exceeds the specified threshold.
                    This will also set the ADC in continuous conversion mode.
        */
        /**************************************************************************/

        protected void startComparator_SingleEnded(byte channel, short threshold)
        {
            // Start with default values
            ushort config = ADS1015_REG_CONFIG_CQUE_1CONV | // Comparator enabled and asserts on 1 match
                            ADS1015_REG_CONFIG_CLAT_LATCH | // Latching mode
                            ADS1015_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                            ADS1015_REG_CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                            ADS1015_REG_CONFIG_DR_1600SPS | // 1600 samples per second (default)
                            ADS1015_REG_CONFIG_MODE_CONTIN; // Continuous conversion mode

            // Set PGA/voltage range
            config |= (ushort)Gain;

            // Set single-ended input channel
            switch (channel)
            {
                case 0:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_0;
                    break;

                case 1:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_1;
                    break;

                case 2:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_2;
                    break;

                case 3:
                    config |= ADS1015_REG_CONFIG_MUX_SINGLE_3;
                    break;
            }

            // Set the high threshold register
            // Shift 12-bit results left 4 bits for the ADS1015
            var thresholdShift = Convert.ToUInt16(threshold << BitShift);
            WriteBytes(ADS1015_REG_POINTER_HITHRESH, new[] { (byte)(thresholdShift >> 8), (byte)(thresholdShift & 0xFF) });

            // Write config register to the ADC
            //this.WriteRegister16Bits(ADS1015_REG_POINTER_CONFIG, config);
            WriteBytes(ADS1015_REG_POINTER_CONFIG, new[] { (byte)(config >> 8), (byte)(config & 0xFF) });
        }

        /**************************************************************************/
        /*!
            @brief  In order to clear the comparator, we need to read the
                    conversion results.  This function reads the last conversion
                    results without changing the config value.
        */
        /**************************************************************************/

        protected short getLastConversionResults()
        {
            // Wait for the conversion to complete
            SleepMilliSeconds(ConversionDelay);

            // Read the conversion results
            var bytes = ReadBytes(ADS1015_REG_POINTER_CONVERT, 2);
            var u16 = (ushort)((bytes[0] << 8) | bytes[1]);
            var res = Convert.ToUInt16(u16 >> BitShift);
            if (BitShift == 0)
            {
                return (short)res;
            }

            // Shift 12-bit results right 4 bits for the ADS1015,
            // making sure we keep the sign bit intact
            if (res > 0x07FF) res |= 0xF000;
            return (short)res;
        }

        public double ConvertToVoltage(short value)
        {
            return value * Vref / 32767;
        }

        public double Normalize(short value)
        {
            return value * Vref / 32767 / 4.8;
        }

        protected void SleepMilliSeconds(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}