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
            _gpio.OpenPin(_resetPin, PinMode.Output);
            _gpio.OpenPin(_dataCommandPin, PinMode.Output);
            _gpio.OpenPin(_backlightPin, PinMode.Output);
            _gpio.Write(_backlightPin, PinValue.High);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}