using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Spi;
using Verdure.Iot.Device;

string input2inch4Path = "LCD_2inch4.jpg";

using SpiDevice sender2inch4Device = SpiDevice.Create(new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 40000000,
    Mode = SpiMode.Mode0
});

using var inch24 = new LCD2inch4(sender2inch4Device);
inch24.Reset();
inch24.Init();
inch24.Clear();
inch24.BlDutyCycle(50);

using (Image<Bgra32> image2inch4 = Image.Load<Bgra32>(input2inch4Path))
{
    using Image<Bgr24> converted2inch4Image = image2inch4.CloneAs<Bgr24>();
    inch24.ShowImage(converted2inch4Image);
}

Console.WriteLine("2inch4 Done");

string input1inch47Path = "LCD_1inch47.jpg";

using SpiDevice sender1inch47Device = SpiDevice.Create(new SpiConnectionSettings(0, 1)
{
    ClockFrequency = 40000000,
    Mode = SpiMode.Mode0
});

using var inch147 = new LCD1inch47(sender1inch47Device);
inch147.Reset();
inch147.Init();
inch147.Clear();
inch147.BlDutyCycle(50);

using (Image<Bgra32> image1inch47 = Image.Load<Bgra32>(input1inch47Path))
{
    using Image<Bgr24> converted1inch47Image = image1inch47.CloneAs<Bgr24>();
    inch147.ShowImage(converted1inch47Image);
}
Console.WriteLine("1inch47 Done");
Console.ReadLine();