using MPSSELight.Protocol;
using System.Collections.Generic;
using System.Threading;

namespace TwoWireDevices
{
    public class TwoWireBase
    {
        private readonly I2cBus Twi;

        public TwoWireBase(I2cBus twi, byte deviceAddress)
        {
            Twi = twi;
            DeviceAddress = deviceAddress;
        }

        public byte DeviceAddress { get; }
        public bool IsConnected => Ping();

        public bool Ping()
        {
            Twi.Start();
            var result = Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false);
            Twi.Stop();
            return result;
        }

        public byte ReadByte(byte registerByte)
        {
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false);
            Twi.SendByteAndCheckACK(registerByte);

            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, true);
            var readByte = Twi.ReceiveByte(false);
            Twi.Stop();
            return readByte;
        }

        public byte ReadByte(ushort registerWord)
        {
            var msb = registerWord >> 8;
            var lsb = registerWord & 0xFF;
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false);
            Twi.SendByteAndCheckACK((byte)msb);
            Twi.SendByteAndCheckACK((byte)lsb);

            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, true);
            var readByte = Twi.ReceiveByte(false);
            Twi.Stop();
            return readByte;
        }

        public byte[] ReadBytes(ushort registerWord, int count)
        {
            var msb = registerWord >> 8;
            var lsb = registerWord & 0xFF;
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            Twi.SendByteAndCheckACK((byte)msb); // SEND Address msb
            Twi.SendByteAndCheckACK((byte)lsb); // SEND Address lsb
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, true);

            var queue = new Queue<byte>();

            for (var i = 0; i < count; i++)
            {
                var readByte = Twi.ReceiveByte(true);
                queue.Enqueue(readByte);
            }

            Twi.Stop();

            return queue.ToArray();
        }

        public byte[] ReadBytes(byte registerByte, int count)
        {
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false);
            Twi.SendByteAndCheckACK(registerByte);
            Thread.Sleep(10);
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, true);

            var queue = new Queue<byte>();

            for (var i = 0; i < count; i++)
            {
                var ack = i < count - 1;
                var readByte = Twi.ReceiveByte(ack);
                queue.Enqueue(readByte);
            }

            Twi.Stop();

            return queue.ToArray();
        }

        public byte[] ReadBytes(int count)
        {
            Twi.Start();
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, true);

            var queue = new Queue<byte>();

            for (var i = 0; i < count; i++)
            {
                var readByte = Twi.ReceiveByte(true);
                queue.Enqueue(readByte);
            }

            Twi.Stop();

            return queue.ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="registerByte"></param>
        /// <param name="value"></param>
        public void WriteByte(byte registerByte, byte value)
        {
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            Twi.SendByteAndCheckACK(registerByte); // SEND REGISTER ID
            Twi.SendByteAndCheckACK(value); // SEND VALUE TO WRITE
            Twi.Stop();
        }

        /// <summary>
        /// </summary>
        /// <param name="registerWord"></param>
        /// <param name="value"></param>
        public void WriteByte(ushort registerWord, byte value)
        {
            var msb = registerWord >> 8;
            var lsb = registerWord & 0xFF;
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            Twi.SendByteAndCheckACK((byte)msb); // SEND REGISTER ID
            Twi.SendByteAndCheckACK((byte)lsb); // SEND REGISTER ID
            Twi.SendByteAndCheckACK(value); // SEND VALUE TO WRITE
            Twi.Stop();
        }

        public void WriteBytes(ushort registerWord, byte[] data)
        {
            var msb = registerWord >> 8;
            var lsb = registerWord & 0xFF;
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            Twi.SendByteAndCheckACK((byte)msb); // SEND Address msb
            Twi.SendByteAndCheckACK((byte)lsb); // SEND Address lsb
            foreach (var value in data) Twi.SendByteAndCheckACK(value); // Send Data
            Twi.Stop();
        }

        public void WriteBytes(byte[] data)
        {
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            foreach (var value in data) Twi.SendByteAndCheckACK(value); // Send Data
            Twi.Stop();
        }

        public void WriteBytes(byte registerByte, byte[] data)
        {
            Twi.Start(); // I2C START
            Twi.SendDeviceAddrAndCheckACK(DeviceAddress, false); // I2C ADDRESS (for write)
            Twi.SendByteAndCheckACK(registerByte); // SEND Address msb
            foreach (var value in data) Twi.SendByteAndCheckACK(value); // SEND Data
            Twi.Stop();
        }
    }
}