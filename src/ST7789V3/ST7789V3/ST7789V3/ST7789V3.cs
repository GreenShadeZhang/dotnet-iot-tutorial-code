using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;

namespace Iot.Device.ST7789V3
{
    public class ST7789V3 : IDisposable
    {
        private int _resetPin;
        private int _dataCommandPin;
        private PwmChannel? _pwmBacklight;

        private GpioController _gpio;
        private SpiDevice _sensor;
        private bool _shouldDispose;


        #region SpiSettings

        /// <summary>
        /// ST7789V3 SPI Clock Frequency
        /// </summary>
        public const int SpiClockFrequency = 40000000;

        /// <summary>
        /// ST7789V3 SPI Mode
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;
        #endregion

        public ST7789V3(int dataCommandPin,
            SpiDevice sensor,
            int resetPin = -1,
            PwmChannel? pwmBacklight = null,
            PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical,
            GpioController? gpioController = null,
            bool shouldDispose = true)
        {
            if (dataCommandPin < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            _dataCommandPin = dataCommandPin;
            _pwmBacklight = pwmBacklight;
            _pwmBacklight?.Start();

            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));

            _gpio = gpioController ?? new GpioController(pinNumberingScheme);
            _resetPin = resetPin;
            _shouldDispose = shouldDispose || gpioController is null;
            Initialize();
        }

        private void Initialize()
        {
            Console.WriteLine("init ok");
            _gpio.OpenPin(_resetPin, PinMode.Output);
            _gpio.OpenPin(_dataCommandPin, PinMode.Output);
        }

        public void SpiWrite(bool isData, ReadOnlySpan<byte> writeData)
        {
            Console.WriteLine($"writeData length:{writeData.Length}");

            _gpio.Write(_dataCommandPin, isData ? PinValue.High : PinValue.Low);

            if (writeData.Length > 4096)
            {
                for (int i = 0; i < 26; i++)
                {
                    var query = writeData[(i * 4096)..((i * 4096) + 4096)];
                    _sensor.Write(query);
                }

                var dataLcdList1 = writeData[(26 * 4096)..110080];

                _sensor.Write(dataLcdList1);
            }
            else
            {
                _sensor.Write(writeData);
            }
            //Span<byte> readBuf = stackalloc byte[writeData.Length];

            //_sensor.TransferFullDuplex(writeData, readBuf);
        }

        public void Init()
        {
            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x36 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x3A }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x05 }));


            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xB2 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0C }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0C }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xB7 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x35 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xBB }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x35 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x2C }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC2 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x01 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC3 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x13 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC4 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x20 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC6 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0F }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xD0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xA4 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xA1 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xE0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x05 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x29 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x3E }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x38 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x12 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x12 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x28 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x30 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xE1 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x07 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0A }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0D }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0B }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x07 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x28 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x3E }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x36 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x14 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x14 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x29 }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x32 }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x21 }));
            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x11 }));
            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x29 }));
        }

        public void SetWindows(int xStart, int yStart, int xEnd, int yEnd)
        {
            // set the X coordinates
            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x2A }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)(((xStart) >> 8) & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((xStart + 34) & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((xEnd - 1 + 34) >> 8 & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((xEnd - 1 + 34) & 0xff) }));

            // set the Y coordinates
            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x2B }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((yStart) >> 8 & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((yStart) & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((yEnd - 1) >> 8 & 0xff) }));
            SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { (byte)((yEnd - 1) & 0xff) }));

            SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x2C }));
        }

        public void Reset()
        {
            _gpio.Write(_resetPin, PinValue.High);
            Thread.Sleep(1);
            Console.WriteLine("reset high");
            _gpio.Write(_resetPin, PinValue.Low);
            Thread.Sleep(1);
            Console.WriteLine("reset low");
            _gpio.Write(_resetPin, PinValue.High);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}