using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;

namespace Iot.Device.ST7789V3
{
    public class ST7789V3 : IDisposable
    {
        private int _resetPin;
        private int _dataCommandPin;
        private int _backlightPin = -1;
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
            //_gpio.OpenPin(_backlightPin, PinMode.Output);
            //_gpio.Write(_backlightPin, PinValue.High);
        }

        public void SpiWrite(bool isData, ReadOnlySpan<byte> writeData)
        {
            Console.WriteLine($"writeData length:{writeData.Length}");

            _gpio.Write(_dataCommandPin, isData ? PinValue.High : PinValue.Low);

            for (int i = 0; i < writeData.Length; i += 4096)
            {
                var query = writeData[i..(i + 4096)];
                _sensor.Write(writeData);
            }

            //if (writeData.Length > 4096)
            //{
            //    for (int i = 0; i < writeData.Length; i += 4096)
            //    {
            //        var query = writeData[i..(i + 4096)];
            //        _sensor.Write(writeData);
            //    }
            //}
            //else
            //{
            //    _sensor.Write(writeData);
            //}
            //Span<byte> readBuf = stackalloc byte[writeData.Length];

            //_sensor.TransferFullDuplex(writeData, readBuf);
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