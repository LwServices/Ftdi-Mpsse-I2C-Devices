using MPSSELight.Devices;
using MPSSELight.Ftdi;
using MPSSELight.Mpsse;
using MPSSELight.Protocol;
using SharpDX.XInput;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using TwoWireDevices.Ads1115;
using TwoWireDevices.Epprom;
using TwoWireDevices.Example.ConsoleColor;
using TwoWireDevices.Pca9685;
using TwoWireDevices.Tcs34725;
using X360Ctrl;

namespace TwoWireDevices.Example
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(FtdiInventory.DeviceListInfo());

            var ftDeviceInfo = FtdiInventory.GetDevices();
            if (ftDeviceInfo.Length == 0)
            {
                Console.WriteLine("No Device");
                Console.ReadKey();
                return;
            }
            var firstSerial = ftDeviceInfo.FirstOrDefault().SerialNumber;

            MpsseDevice.MpsseParams mpsseParams = new MpsseDevice.MpsseParams
            {
                Latency = 16,
                ReadTimeout = 50,
                WriteTimeout = 50,
                clockDevisor = 49 * 6
            };

            using (MpsseDevice mpsse = new FT232H(firstSerial, mpsseParams))
            {
                Console.WriteLine("MPSSE init success with clock frequency {0:0.0} Hz", mpsse.ClockFrequency);

                var i2c = new I2cBus(mpsse);
                var gpio = new Gpio(mpsse);

                Selector(i2c, gpio);
            }
        }

        private static void Selector(I2cBus i2c, Gpio gpio)
        {
            Console.WriteLine("a = Arduino Slave");
            Console.WriteLine("b = Mpu9250");
            Console.WriteLine("d = Digital ADS1115 (Analog In)");
            Console.WriteLine("e = AT24C32 EEPROM");
            Console.WriteLine("g = GPIO");

            Console.WriteLine("i = Input");
            Console.WriteLine("l = LM75 Temperature");
            Console.WriteLine("m = MCP4725 (Analog Out)");
            Console.WriteLine("n = Nunchuk");
            Console.WriteLine("s = I2C Scan");
            Console.WriteLine("t = tcs 34725");

            Console.WriteLine("r = Robot");
            Console.WriteLine("v = VCNL 4010");
            Console.WriteLine("w = VL53L0X");

            switch (Console.ReadKey().KeyChar)
            {
                case 'a':
                    ArduinoSlave(i2c);
                    break;

                //                case 'b':
                //                    Mpu9250(i2c);
                //                    break;

                case 'd':
                    Ads1115(i2c);
                    break;

                case 'e':
                    At24c32(i2c);
                    break;

                case 'g':
                    GpioTest(i2c, gpio);
                    break;

                case 'i':
                    Inputtest();
                    break;

                case 'l':
                    Lm75(i2c, gpio);
                    break;

                case 'm':
                    Mcp4725(i2c);
                    break;

                case 'n':
                    Nunchuck(i2c);
                    break;

                case 'r':
                    Roboter(i2c);
                    break;

                case 's':
                    Scan(i2c);
                    break;

                case 't':
                    Tcs34725(i2c);
                    break;

                case 'v':
                    Vcnl4010(i2c);
                    break;

                case 'w':
                    Vl53l0x(i2c);
                    break;
            }
        }

        private static void GpioTest(I2cBus i2c, Gpio gpio)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 5;
            timer.Elapsed += (sender, args) =>
            {
                gpio.Out0 = !gpio.Out0;
                gpio.Multiplex();
                gpio.SetLowGpio();
            };

            timer.AutoReset = true;
            timer.Enabled = true;
            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                {
                    keyChar = Console.ReadKey().KeyChar;
                    if (keyChar == '1')
                    {
                        gpio.Out0 = false;
                    }

                    if (keyChar == '2')
                    {
                        gpio.Out0 = true;
                    }

                    if (keyChar == '3')
                    {
                        gpio.Out1 = false;
                    }

                    if (keyChar == '4')
                    {
                        gpio.Out1 = true;
                    }

                    if (keyChar == '5')
                    {
                        gpio.Out2 = false;
                    }

                    if (keyChar == '6')
                    {
                        gpio.Out2 = true;
                    }
                }
                Thread.Sleep(2);
            } while (keyChar != 'x');
        }

        private static void Tcs34725(I2cBus i2c)
        {
            var tcs = new Tcs34725.Tcs34725(i2c, Tcs34725IntegrationTime.TCS34725_INTEGRATIONTIME_50MS, Tcs34725Gain.TCS34725_GAIN_16X);

            tcs.Init();
            bool even = false;
            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                tcs.Enable();
                tcs.GetData();
                tcs.Disable();

                ConsoleColorChanger.SetColor(System.ConsoleColor.Blue, tcs.Red, tcs.Green, tcs.Blue);
                Console.ForegroundColor = System.ConsoleColor.Blue;
                Console.WriteLine(tcs);
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        //        private static void Mpu9250(I2cBus i2c)
        //        {
        //            var mpu = new MPU9250(i2c);
        //            if (!mpu.Ping())
        //            {
        //                Console.WriteLine("NO Ping");
        //                Console.ReadKey();
        //            }
        //
        //            if (!mpu.whoAmI())
        //            {
        //                Console.WriteLine("Who Am I is not 0x71");
        //                Console.ReadKey();
        //            }
        //
        //            mpu.initMPU9250();
        //
        //            mpu.calibrateMPU9250();
        //
        //            bool even = false;
        //            char keyChar = ' ';
        //            do
        //            {
        //                if (Console.KeyAvailable)
        //                    keyChar = Console.ReadKey().KeyChar;
        //                var gyro = mpu.readGyroData();
        //
        //                //mpu.readSensor();
        //                Console.WriteLine($"Gx {gyro.gx} Gy {gyro.gy} Gz {gyro.gz}");
        //                Thread.Sleep(300);
        //            } while (keyChar != 'x');
        //        }

        private static void Vl53l0x(I2cBus i2c)
        {
            var tcs = new VL53L0X.VL53L0X(i2c);

            bool even = false;
            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                Console.WriteLine(tcs);
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        private static void Vcnl4010(I2cBus i2c)
        {
            var vcnl = new Vcnl4010.Vcnl4010(i2c);

            var begin = vcnl.begin();
            Console.WriteLine($"Begin {begin}");

            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                var ambient = vcnl.readAmbient();
                var proxy = vcnl.readProximity();
                Console.WriteLine($"Ambient {ambient}  Proxy {proxy}");
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        private static void Ads1115(I2cBus i2c)
        {
            var ads1115 = new ADS1115(i2c, 0x4A);

            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                var ch0 = ads1115.ReadAdcSingleEnded(0);
                Thread.Sleep(10);

                var ch1 = ads1115.ReadAdcSingleEnded(1);
                Thread.Sleep(10);

                var v0 = ads1115.ConvertToVoltage(ch0);
                var v1 = ads1115.ConvertToVoltage(ch1);

                Console.WriteLine($"v0 {v0:F3} {ch0} v1 {v1:F3} {ch1}");
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        private static void Mcp4725(I2cBus i2c)
        {
            var mcp = new Mcp4725.Mcp4725(i2c);
            var ads1115 = new ADS1115(i2c, 0x4A);

            char keyChar = ' ';
            UInt16 outVar = 0;
            do
            {
                if (Console.KeyAvailable)
                {
                    keyChar = Console.ReadKey().KeyChar;
                    if (keyChar == '+')
                        outVar += 10;

                    if (keyChar == '-')
                        outVar -= 10;
                }

                mcp.SetVoltage(outVar, false);
                var ch0 = ads1115.ReadAdcSingleEnded(0);
                Thread.Sleep(10);

                var v0 = ads1115.ConvertToVoltage(ch0);

                Console.WriteLine($"OutVar {outVar} In {v0}");
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        private static void Nunchuck(I2cBus i2c)
        {
            var nc = new Nunchuk.Nunchuk(i2c);
            nc.Init();

            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                nc.Update();

                Console.WriteLine(nc);
                Thread.Sleep(300);
            } while (keyChar != 'x');
        }

        private static void ArduinoSlave(I2cBus i2c)
        {
            var twi = new TwoWireBase(i2c, 0x08);

            byte[] array = Encoding.ASCII.GetBytes("Lech The Best\n");

            Console.WriteLine($"{array} is {array.Length} long");
            char keyChar1 = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar1 = Console.ReadKey().KeyChar;

                var ret0 = twi.ReadBytes(0x00, 6);
                Console.WriteLine($"Recesived0 {BitConverter.ToString(ret0, 0)} as {Encoding.ASCII.GetString(ret0)}");

                var ret1 = twi.ReadBytes(6);
                Console.WriteLine($"Recesived1 {BitConverter.ToString(ret1, 0)} as {Encoding.ASCII.GetString(ret1)}");

                var ret2 = twi.ReadBytes(0x0102, 6);
                Console.WriteLine($"Recesived2 {BitConverter.ToString(ret2, 0)} as {Encoding.ASCII.GetString(ret2)}");

                var ret3 = (char)twi.ReadByte(0x01);
                Console.WriteLine($"ReadByte {ret3}");

                var ret4 = (char)twi.ReadByte(0x0102);
                Console.WriteLine($"ReadByte {ret4}");
                // twi.WriteBytes(0x41, array);

                //                    foreach (var letter in array)
                //                    {
                //                        twi.WriteByte(0x41, (byte)letter);
                //                    }
                //var b = twi.ReadByte(0x00);
                //Console.WriteLine(b);
                //                var list = new List<char>();
                //                twi.Twi.Start();
                //                twi.Twi.SendDeviceAddrAndCheckACK(0x08, true);
                //                //var read = twi.Twi.ReadNBytes(6);
                //
                //                for (int i = 0; i < 6; i++)
                //                {
                //                    var r = twi.Twi.ReceiveByte(true);
                //                    list.Add((char)r);
                //                }
                //
                //                twi.Twi.Stop();

                //Console.WriteLine($"Recesived {BitConverter.ToString(read, 0)}");
                //Console.WriteLine($"Recesived {String.Join("", list)}");

                Thread.Sleep(1000);
            } while (keyChar1 != 'x');

            Console.ReadKey();
            return;
        }

        private static void Inputtest()
        {
            var ctrl = new XInputController(UserIndex.One);

            if (!ctrl.IsConnected)
            {
                //return;
            }

            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                    keyChar = Console.ReadKey().KeyChar;

                ctrl.Update();
                Console.WriteLine(ctrl);
            } while (keyChar != 'x');
        }

        private static void At24c32(I2cBus i2c)
        {
            var at = new AT24C32(i2c, 0x50);

            at.WriteByte((ushort)10, 0x42);
            Thread.Sleep(10);
            var ret = at.ReadByte((ushort)10);
            Console.WriteLine($"ret {ret:x}");

            at.WriteBytes(10, new byte[] { 0, 8, 15 });
            Thread.Sleep(10);

            var result = at.ReadBytes((ushort)10, 3);
            var disp = BitConverter.ToString(result);
            Console.WriteLine(disp);
        }

        private static void Lm75(I2cBus i2c, Gpio gpio)
        {
            var lm = new Lm75.Lm75(i2c, 0x48);
            lm.SetTos(75);
            lm.SetThyst(80);
            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                {
                    keyChar = Console.ReadKey().KeyChar;

                    switch (keyChar)
                    {
                        case '1':
                            lm.SetTos(15);
                            Thread.Sleep(10);
                            lm.SetThyst(20);
                            Thread.Sleep(10);
                            break;

                        case '2':
                            lm.SetTos(20);
                            Thread.Sleep(10);
                            lm.SetThyst(25);
                            Thread.Sleep(10);
                            break;

                        case '3':
                            lm.SetTos(25);
                            Thread.Sleep(10);
                            lm.SetThyst(30);
                            Thread.Sleep(10);
                            break;

                        case '4':
                            lm.SetTos(25);
                            Thread.Sleep(10);
                            lm.SetThyst(125);
                            Thread.Sleep(10);
                            break;

                        case '0':
                            lm.SetConfig(0x00);
                            Thread.Sleep(10);
                            break;
                    }
                }

                if (lm.IsConnected)
                {
                    var temperature = lm.GetTemperature();
                    Thread.Sleep(10);
                    var tos = lm.GetTos();
                    Thread.Sleep(10);
                    var thyst = lm.GetThyst();
                    Thread.Sleep(10);

                    //var os = gpio.GetLowGpio();

                    Console.WriteLine($"Temp:{temperature,10} Tos: {tos,10} Thyst: {thyst,10}");
                }
                else
                {
                    Console.WriteLine("Ping Failed");
                }

                Thread.Sleep(500);
            } while (keyChar != 'x');
        }

        private static void Roboter(I2cBus i2c)
        {
            var pca9685 = new Pca9685.Pca9685(i2c, 0x40, 50);

            var axis1 = new ServoAxis(0, pca9685, 158, 529); // Savöx
            var axis2 = new ServoAxis(1, pca9685, 172, 519); // Spektrum
            var axis3 = new ServoAxis(2, pca9685, 126, 528); // Tower

            //                var axis4 = new ServoAxis(3, pca9685, 150, 650);
            //                var axis5 = new ServoAxis(4, pca9685, 150, 650);
            //                var axis6 = new ServoAxis(5, pca9685, 150, 650);

            var ads1115 = new ADS1115(i2c, 0x4A);

            var ctrl = new XInputController(UserIndex.One);

            if (!ctrl.IsConnected)
            {
                Console.WriteLine("No Controller Conected");
                Console.ReadKey();

                return;
            }
            ctrl.StartAutoUpdate();
            ctrl.ButtonPressed += axis3.OnButtonPressed;
            ctrl.Updated += axis1.OnUpdate;

            char keyChar = ' ';
            do
            {
                if (Console.KeyAvailable)
                {
                    keyChar = Console.ReadKey().KeyChar;
                }

                Console.Clear();
                if (ctrl.A)
                {
                    axis1.MoveAbsolute(ctrl.LeftThumbX);
                    axis2.MoveAbsolute(ctrl.LeftThumbY);
                }
                else
                {
                    var ch0 = ads1115.ReadAdcSingleEnded(0);
                    var ch1 = ads1115.ReadAdcSingleEnded(1);

                    var pot0 = ads1115.Normalize(ch0);
                    var pot1 = ads1115.Normalize(ch1);
                    axis1.MoveAbsolute(pot0);
                    axis2.MoveAbsolute(pot1);
                }
                //axis2.MoveAbsolute(ctrl.LeftThumbY);
                axis3.MoveAbsolute(ctrl.RightThumbY);
                //                    axis4.MoveAbsolute(ctrl.RightThumbX);
                //                    axis5.MoveAbsolute(ctrl.LeftTrigger);
                //                    axis6.MoveAbsolute(ctrl.RightTrigger);
            } while (!ctrl.Start && keyChar != 'x');
        }

        private static void Scan(I2cBus twi)
        {
            char keyChar;
            do
            {
                Console.Clear();
                keyChar = Console.KeyAvailable ? Console.ReadKey().KeyChar : ' ';
                // Scan
                for (int i = 3; i < 127; i++)
                {
                    twi.Start();
                    var result = twi.SendDeviceAddrAndCheckACK((byte)i, false);
                    if (result)
                        Console.WriteLine($"I2C Address {i,3} 0x{i:x}");
                    twi.Stop();
                }

                Console.WriteLine("Press x to exit");
                Thread.Sleep(1000);
            } while (keyChar != 'x');

            Console.ReadKey();
        }
    }
}