using MPSSELight.Protocol;
using System.Threading;

namespace TwoWireDevices.Vcnl4010
{
    public class Vcnl4010 : TwoWireBase
    {
        public enum vcnl4010_freq
        {
            VCNL4010_1_95 = 0,
            VCNL4010_3_90625 = 1,
            VCNL4010_7_8125 = 2,
            VCNL4010_16_625 = 3,
            VCNL4010_31_25 = 4,
            VCNL4010_62_5 = 5,
            VCNL4010_125 = 6,
            VCNL4010_250 = 7
        }

        private const int VCNL4010_I2CADDR_DEFAULT = 0x13;

        // commands and constants
        private const int VCNL4010_COMMAND = 0x80;

        private const int VCNL4010_PRODUCTID = 0x81;
        private const int VCNL4010_PROXRATE = 0x82;
        private const int VCNL4010_IRLED = 0x83;
        private const int VCNL4010_AMBIENTPARAMETER = 0x84;
        private const int VCNL4010_AMBIENTDATA = 0x85;
        private const int VCNL4010_PROXIMITYDATA = 0x87;
        private const int VCNL4010_INTCONTROL = 0x89;
        private const int VCNL4010_PROXINITYADJUST = 0x8A;
        private const int VCNL4010_INTSTAT = 0x8E;
        private const int VCNL4010_MODTIMING = 0x8F;

        public const byte PROX_MEASUREMENT_RATE_31 = 0x04;

        public const byte COMMAND_SELFTIMED_MODE_ENABLE = 0x01;

        public const byte COMMAND_PROX_ENABLE = 0x02;
        public const byte COMMAND_AMBI_ENABLE = 0x04;
        public const byte COMMAND_MASK_PROX_DATA_READY = 0x20;

        public const byte AMBI_PARA_AVERAGE_32 = 0x05; // DEFAULT
        public const byte AMBI_PARA_AUTO_OFFSET_ENABLE = 0x08; // DEFAULT enable
        public const byte AMBI_PARA_MEAS_RATE_2 = 0x10; // DEFAULT
        public const byte INTERRUPT_THRES_SEL_PROX = 0x00;
        public const byte INTERRUPT_THRES_ENABLE = 0x02;
        public const byte INTERRUPT_COUNT_EXCEED_1 = 0x00; // DEFAULT

        private const int VCNL4010_MEASUREAMBIENT = 0x10;
        private const int VCNL4010_MEASUREPROXIMITY = 0x08;
        private const int VCNL4010_AMBIENTREADY = 0x40;
        private const int VCNL4010_PROXIMITYREADY = 0x20;

        public Vcnl4010(I2cBus twi, byte deviceAddress) : base(twi, deviceAddress)
        {
        }

        public Vcnl4010(I2cBus twi) : this(twi, VCNL4010_I2CADDR_DEFAULT)
        {
        }

        public bool begin()
        {
            var rev = ReadByte(VCNL4010_PRODUCTID);
            if ((rev & 0xF0) != 0x20)
            {
                //  return false;
            }

            setLEDcurrent(20); // 200 mA
            //setFrequency(vcnl4010_freq.VCNL4010_250); // 16.625 readings/second
            WriteByte(VCNL4010_PROXRATE, PROX_MEASUREMENT_RATE_31);
            WriteByte(VCNL4010_COMMAND, COMMAND_PROX_ENABLE | COMMAND_AMBI_ENABLE | COMMAND_SELFTIMED_MODE_ENABLE);
            //WriteByte(VCNL4010_INTCONTROL, 0x08);
            WriteByte(VCNL4010_INTCONTROL,
                INTERRUPT_THRES_SEL_PROX | INTERRUPT_THRES_ENABLE | INTERRUPT_COUNT_EXCEED_1);
            WriteByte(VCNL4010_AMBIENTPARAMETER,
                AMBI_PARA_AVERAGE_32 | AMBI_PARA_AUTO_OFFSET_ENABLE | AMBI_PARA_MEAS_RATE_2);
            WriteByte(VCNL4010_PROXINITYADJUST, 0x81);
            return true;
        }

        public byte getLEDcurrent()
        {
            return ReadByte(VCNL4010_IRLED);
        }

        public void setFrequency(vcnl4010_freq freq)
        {
            WriteByte(VCNL4010_MODTIMING, (byte)freq);
        }

        public void setLEDcurrent(byte current_10mA)
        {
            if (current_10mA > 20) current_10mA = 20;
            WriteByte(VCNL4010_IRLED, current_10mA);
        }

        public int readProximity()
        {
            var i = ReadByte(VCNL4010_INTSTAT);
            WriteByte(VCNL4010_INTSTAT, (byte)(i | 0x80));
            var cmd = ReadByte(VCNL4010_COMMAND);
            WriteByte(VCNL4010_COMMAND, (byte)(cmd | VCNL4010_MEASUREPROXIMITY));
            while (true)
            {
                //Serial.println(read8(VCNL4010_INTSTAT), HEX);
                var ready = ReadByte(VCNL4010_COMMAND);
                //Serial.print("Ready = 0x"); Serial.println(result, HEX);
                if ((ready & VCNL4010_PROXIMITYREADY) == VCNL4010_PROXIMITYREADY)
                {
                    var result = ReadBytes(VCNL4010_PROXIMITYDATA, 2);
                    return (result[0] << 8) | result[1];
                }

                Thread.Sleep(10);
            }
        }

        public int readAmbient()
        {
            var intstat = ReadByte(VCNL4010_INTSTAT);
            //WriteByte(VCNL4010_INTSTAT, (byte)(intstat & ~0x40));

            //WriteByte(VCNL4010_COMMAND, VCNL4010_MEASUREAMBIENT);
            while (true)
            {
                //Serial.println(read8(VCNL4010_INTSTAT), HEX);
                var ready = ReadByte(VCNL4010_COMMAND);
                //Serial.print("Ready = 0x"); Serial.println(result, HEX);
                if ((ready & VCNL4010_AMBIENTREADY) == VCNL4010_AMBIENTREADY)
                {
                    var result = ReadBytes(VCNL4010_AMBIENTDATA, 2);
                    return (result[0] << 8) | result[1];
                }

                Thread.Sleep(10);
            }
        }
    }
}