using Iot.Device.ST7789V3;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Device.Spi;

var resetPin = 27;
var dataCommandPin = 25;
var backlightPin = 18;
var blFreq = 1000;
// SPI0 CS0
SpiConnectionSettings senderSettings = new(0, 0)
{
    ClockFrequency = ST7789V3.SpiClockFrequency,
    Mode = ST7789V3.SpiMode
};

using SpiDevice senderDevice = SpiDevice.Create(senderSettings);

PwmChannel pwmChannel = new SoftwarePwmChannel(pinNumber: backlightPin, frequency: blFreq);
//var pwmChannel = PwmChannel.Create(0, 0, frequency: blFreq);

var lcd = new ST7789V3(dataCommandPin, senderDevice, resetPin, pwmChannel, shouldDispose: false);

lcd.Reset();

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x36 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x3A }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x05 }));


lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xB2 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0C }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0C }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xB7 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x35 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xBB }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x35 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x2C }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC2 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x01 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC3 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x13 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC4 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x20 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xC6 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0F }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xD0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xA4 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xA1 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xE0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x00 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x04 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x05 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x29 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x3E }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x38 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x12 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x12 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x28 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x30 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0xE1 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0xF0 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x07 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0A }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0D }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x0B }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x07 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x28 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x33 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x3E }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x36 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x14 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x14 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x29 }));
lcd.SpiWrite(true, new ReadOnlySpan<byte>(new byte[] { 0x32 }));

lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x21 }));
lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x11 }));
lcd.SpiWrite(false, new ReadOnlySpan<byte>(new byte[] { 0x29 }));

lcd.SetWindows(0, 0, 320, 170);

//var imageFilePath = "./Pic/LCD_1inch47.jpg";
var imageFilePath = "./Pic/verdure90.png";
using var image = Image.Load<Rgba32>(imageFilePath);

//image.Mutate(x => x.Resize(new Size(172, 320)));

image.Mutate(x => x.BlackWhite());

var colWhite = new Rgba32(255, 255, 255);
var width = 172;
var result = new byte[504];

for (var pos = 0; pos < result.Length; pos++)
{
    byte toStore = 0;
    for (int bit = 0; bit < 8; bit++)
    {
        var x = pos % width;
        var y = pos / width * 8 + bit;
        toStore = (byte)(toStore | ((image[x, y] == colWhite ? 0 : 1) << bit));
    }

    result[pos] = toStore;
}

//var dataLcd = new byte[172 * 320 * 3];

//for (int i = 0; i < dataLcd.Length; i++)
//{
//    dataLcd[i] = 0x00;
//}
for (int i = 0; i < 26; i++)
{
    var dataLcdList = new byte[4096];

    for (int j = 0; j < dataLcdList.Length; j++)
    {
        dataLcdList[i] = 0xFF;
    }
    lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataLcdList));
}
var dataLcdList1 = new byte[3584];

for (int j = 0; j < dataLcdList1.Length; j++)
{
    dataLcdList1[j] = 0xFF;
}
lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataLcdList1));


//var resultString = $"var bitmap = new byte[] {{{String.Join(",", result.Select(b => $"0x{b.ToString("X2")}"))}}}";
//Console.WriteLine(resultString);
Console.ReadKey();
