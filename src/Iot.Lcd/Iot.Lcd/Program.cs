using Iot.Lcd;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string inputPath = "LCD_2inch4.jpg";
string outputPath = "output.png";
string output2Path = "output2.png";
using (Image<Bgra32> image = Image.Load<Bgra32>(inputPath))
{


    using (Image<Bgr24> convertedImage = image.CloneAs<Bgr24>())
    {
        var bytes = Helper.GetImageBytes(convertedImage);
        // 将字节数据转成十六进制写到文档文件里
        //string hexString = BitConverter.ToString(bytes).Replace("-", ",");
        //File.WriteAllText("image_hex1.txt", hexString);
        convertedImage.Save(output2Path);
    }
}

Console.WriteLine("Image conversion from Bgra32 to Bgr24 completed.");