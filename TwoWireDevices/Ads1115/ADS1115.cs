using MPSSELight.Protocol;

namespace TwoWireDevices.Ads1115
{
    public class ADS1115 : ADS1015
    {
        /**************************************************************************/
        /*!
            @brief  Instantiates a new ADS1115 class w/appropriate properties
        */
        /**************************************************************************/

        public ADS1115(I2cBus i2c)
            : this(i2c, ADS1015_ADDRESS)
        {
        }

        public ADS1115(I2cBus i2c, byte address)
            : base(i2c, address)
        {
            ConversionDelay = ADS1115_CONVERSIONDELAY;
            BitShift = 0;
            Gain = AdsGain.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
        }
    }
}