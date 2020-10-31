namespace TwoWireDevices.Pca9685
{
    public interface IPca9685
    {
        void SetPwm(byte channel, int on, int off);

        void Reset();
    }
}