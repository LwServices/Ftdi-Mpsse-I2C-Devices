using SharpDX.XInput;
using System;
using X360Ctrl;

namespace TwoWireDevices.Pca9685
{
    public class ServoAxis
    {
        private readonly byte _channel;

        private readonly IPca9685 _pca;
        private double _setPoint;

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="pca"></param>
        /// <param name="min"></param>
        /// <param name="maxHardwareLimit"></param>
        public ServoAxis(byte channel, IPca9685 pca, int minHardwareLimit, int maxHardwareLimit)
        {
            _channel = channel;
            _pca = pca;
            MinHardwareLimit = minHardwareLimit;
            MaxHardwareLimitHardwareLimit = maxHardwareLimit;
        }

        public int MinHardwareLimit { get; set; }

        public int MaxHardwareLimitHardwareLimit { get; set; }

        public double MinSoftwareLimit { get; set; } = 0;

        public double MaxSoftwareLimit { get; set; } = 1;

        public double SetPoint
        {
            get => _setPoint;
            set
            {
                _setPoint = value;
                MoveAbsolute(_setPoint);
            }
        }

        public double Angle { get; set; }
        public double MaxAngle { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="setPoint"></param>
        public void MoveAbsolute(double setPoint)
        {
            if (setPoint < 0)
                setPoint = 0;
            if (setPoint > 1)
                setPoint = 1;

            if (setPoint < MinSoftwareLimit)
                setPoint = MinSoftwareLimit;
            if (setPoint > MaxSoftwareLimit)
                setPoint = MaxSoftwareLimit;

            var pulse = Normalize(setPoint, 1, 0, MaxHardwareLimitHardwareLimit, MinHardwareLimit);

            Console.WriteLine($"Channel {_channel,2} setPoint: {setPoint:F2} pulse {pulse,5} min {MinHardwareLimit,5} maxHardwareLimit {MaxHardwareLimitHardwareLimit,5}");

            _pca.SetPwm(_channel, 0, (int)pulse);
        }

        public void MoveRelative(double delta)
        {
            _setPoint += delta;
            MoveAbsolute(_setPoint);
        }

        private double Normalize(double x, double max, double min, double new_max, double new_min)
        {
            return (x - min) * (new_max - new_min) / (max - min) + new_min;
        }

        /// <summary>
        ///     Adjust Limits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnButtonPressed(object sender, EventArgs e)
        {
            if (e is ButtonEventArgs b)
                switch (b.Button)
                {
                    case GamepadButtonFlags.A:
                        MinHardwareLimit--;
                        break;

                    case GamepadButtonFlags.B:
                        MinHardwareLimit++;
                        break;

                    case GamepadButtonFlags.X:
                        MaxHardwareLimitHardwareLimit--;
                        break;

                    case GamepadButtonFlags.Y:
                        MaxHardwareLimitHardwareLimit++;
                        break;
                }
        }

        public void OnUpdate(object sender, EventArgs e)
        {
            if (sender is XInputController controller)
            {
                var bla = controller.RightThumbX;
                // controller.MoveAbsolute();
            }
        }
    }
}