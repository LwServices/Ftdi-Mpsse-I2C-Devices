using MPSSELight.Protocol;
using System;
using System.Threading;

namespace TwoWireDevices.Pca9685
{
    /// <summary>
    ///     https://learn.adafruit.com/16-channel-pwm-servo-driver?view=all
    ///     https://github.com/adafruit/Adafruit-PWM-Servo-Driver-Library
    /// </summary>
    public class Pca9685 : TwoWireBase, IPca9685
    {
        // Constants

        private const int PCA9685_I2CDEFAULTADDR = 0x40; /** Device default slave address */
        private const double CLOCK_FREQ = 25000000;

        private const ushort PULSE_RESOLUTION = 4096;

        private const byte ALLCALLADR = 0x05; //LED All Call I2C-bus address

        private const byte LED_MULTIPLYER = 4; // For the other 15 channels
        private const byte ALLLED_ON_L = 0xFA; //load all the LEDn_ON registers, byte 0 (turn 0-7 channels on)
        private const byte ALLLED_ON_H = 0xFB; //load all the LEDn_ON registers, byte 1 (turn 8-15 channels on)
        private const byte ALLLED_OFF_L = 0xFC; //load all the LEDn_OFF registers, byte 0 (turn 0-7 channels off)
        private const byte ALLLED_OFF_H = 0xFD; //load all the LEDn_OFF registers, byte 1 (turn 8-15 channels off)

        // Register
        private const byte PCA9685_MODE1 = 0x00;

        private const byte PCA9685_MODE2 = 0x01;

        private const byte PCA9685_PRESCALE = 0xFE;

        private const byte LED0_OFF_H = 0x09;

        private const byte LED0_OFF_L = 0x08;

        private const byte LED0_ON_H = 0x07;

        private const byte LED0_ON_L = 0x06;

        private const byte PCA9685_SUBADR1 = 0x02;

        private const byte PCA9685_SUBADR2 = 0x03;

        private const byte PCA9685_SUBADR3 = 0x04;
        private float _pwmFrequency;

        public Pca9685(I2cBus twi, float pwmFrequency) : this(twi, PCA9685_I2CDEFAULTADDR, pwmFrequency)
        {
        }

        public Pca9685(I2cBus twi, byte i2cAddress, float pwmFrequency) : base(twi, i2cAddress)
        {
            PwmFrequency = pwmFrequency;
            Reset();
            SetPwmFreq(PwmFrequency);
            Reset();
        }

        /// <summary>
        ///     Frequency
        /// </summary>
        public float PwmFrequency
        {
            get => _pwmFrequency;
            set => SetPwmFreq(value);
        }

        /// <summary>
        ///     Reset Controller
        /// </summary>
        public void Reset()
        {
            WriteByte(PCA9685_MODE1, 0x80);
        }

        /// <summary>
        ///     Set PWM
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <param name="on">On Time</param>
        /// <param name="off">Off Time</param>
        public void SetPwm(byte channel, int on, int off)
        {
            WriteByte(PCA9685_MODE1, (byte)RegMode1Bits.None);
            WriteByte((byte)(LED0_ON_L + LED_MULTIPLYER * channel), (byte)on);
            WriteByte((byte)(LED0_ON_H + LED_MULTIPLYER * channel), (byte)(on >> 8));
            WriteByte((byte)(LED0_OFF_L + LED_MULTIPLYER * channel), (byte)off);
            WriteByte((byte)(LED0_OFF_H + LED_MULTIPLYER * channel), (byte)(off >> 8));
        }

        /// <summary>
        ///     Set Frequency
        /// </summary>
        /// <param name="pwmFrequency">Frequency in Hz</param>
        private void SetPwmFreq(float pwmFrequency)
        {
            _pwmFrequency = pwmFrequency;
            WriteByte(PCA9685_MODE1, 0x10); //sleep

            var prescale = (byte)Math.Floor(CLOCK_FREQ / PULSE_RESOLUTION / pwmFrequency - 0.5);
            WriteByte(PCA9685_PRESCALE, prescale);
            WriteByte(PCA9685_MODE2, 0x04);

            Thread.Sleep(10);

            WriteByte(PCA9685_MODE1, 0x80);
            Thread.Sleep(10);
        }

        [Flags]
        private enum RegMode1Bits : byte
        {
            None = 0,
            AllCall = 1 << 0,
            Sub3 = 1 << 1,
            Sub2 = 1 << 2,
            Sub1 = 1 << 3,
            Sleep = 1 << 4,
            AutoIncr = 1 << 5,
            ExtClk = 1 << 6,
            Restart = 1 << 7
        }

        [Flags]
        private enum RegMode2Bits : byte
        {
            None = 0,
            Outne0 = 1 << 0,
            Outne1 = 1 << 1,
            OutDrv = 1 << 2,
            Och = 1 << 3,
            Invert = 1 << 4,
            Temp1 = 1 << 5,
            Temp2 = 1 << 6,
            Temp3 = 1 << 7
        }
    }
}