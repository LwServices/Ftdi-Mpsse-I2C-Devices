using MPSSELight.Protocol;
using System;

namespace TwoWireDevices.Nunchuk
{
    /// <inheritdoc />
    /// http://wiibrew.org/wiki/Wiimote/Extension_Controllers
    /// http://wiibrew.org/wiki/Wiimote/Extension_Controllers/Nunchuck
    /// https://github.com/infusion/Fritzing/blob/master/Nunchuk/Nunchuk.h
    public class Nunchuk : TwoWireBase
    {
        private const int NUNCHUK_I2CDEFAULTADDRESS = 0x52;

        public Nunchuk(I2cBus twi, byte deviceAddress) : base(twi, deviceAddress)
        {
        }

        public Nunchuk(I2cBus twi) : this(twi, NUNCHUK_I2CDEFAULTADDRESS)
        {
        }

        public int AnalogX { get; set; }
        public int AnalogY { get; set; }
        public int AccelX { get; set; }
        public int AccelY { get; set; }
        public int AccelZ { get; set; }
        public bool ZButton { get; set; }
        public bool CButton { get; set; }

        public void Init()
        {
            //            WriteByte(0x55, 0xF0);
            //            WriteByte(0x00, 0xFB);

            WriteByte(0x55, 0xF0);
            WriteByte(0x00, 0xFB);

            var serial = ReadBytes(0xFA, 6);

            var bc = BitConverter.ToString(serial);
            Console.WriteLine(bc);
            Update();
        }

        public void Update()
        {
            var count = 0;
            //int values[6];

            //Wire.requestFrom(NUNCHUK_I2CDEFAULTADDRESS, 6);
            var values = ReadBytes(6);
            //            while (Wire.available())
            //            {
            //                values[count] = Wire.read();
            //                count++;
            //            }

            AnalogX = values[0];
            AnalogY = values[1];
            AccelX = (values[2] << 2) | ((values[5] >> 2) & 3);
            AccelY = (values[3] << 2) | ((values[5] >> 4) & 3);
            AccelZ = (values[4] << 2) | ((values[5] >> 6) & 3);
            ZButton = ((values[5] >> 0) & 0x01) != 0;
            CButton = ((values[5] >> 1) & 0x01) != 0;

            WriteByte(0x00, 0x00);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(AnalogX)}: {AnalogX}, {nameof(AnalogY)}: {AnalogY}, {nameof(AccelX)}: {AccelX}, {nameof(AccelY)}: {AccelY}, {nameof(AccelZ)}: {AccelZ}, {nameof(ZButton)}: {ZButton}, {nameof(CButton)}: {CButton}";
        }
    }
}