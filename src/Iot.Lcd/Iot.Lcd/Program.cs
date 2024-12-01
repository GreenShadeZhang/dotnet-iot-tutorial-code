using Iot.Lcd;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Spi;
using Verdure.Iot.Device;

string inputPath = "LCD_2inch4.jpg";

using SpiDevice senderDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ChipSelectLine = 0,
    ClockFrequency = 40000000,
    Mode = SpiMode.Mode0
});

using var inch24 = new LCD2inch4(senderDevice);
inch24.Reset();
inch24.Init();
inch24.Clear();
inch24.BlDutyCycle(50);

using (Image<Bgra32> image = Image.Load<Bgra32>(inputPath))
{
    using (Image<Bgr24> convertedImage = image.CloneAs<Bgr24>())
    {
        inch24.ShowImage(convertedImage);
    }
}
Console.WriteLine("Done");
Console.ReadLine();