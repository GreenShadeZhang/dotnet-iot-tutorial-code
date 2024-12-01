using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Spi;

namespace Verdure.Iot.Device;
public class LCD1inch47 : LcdConfig
{
    public const int Width = 240;
    public const int Height = 320;
    public LCD1inch47(SpiDevice spi) : base(spi, 40000000, 27, 25, 18, 1000)
    {
    }
    public void Command(byte cmd)
    {
        DigitalWrite(DC_PIN, false);
        SpiWriteByte([cmd]);
    }

    public void Data(byte val)
    {
        DigitalWrite(DC_PIN, true);
        SpiWriteByte([val]);
    }

    public void Reset()
    {
        DigitalWrite(RST_PIN, true);
        Thread.Sleep(10);
        DigitalWrite(RST_PIN, false);
        Thread.Sleep(10);
        DigitalWrite(RST_PIN, true);
        Thread.Sleep(10);
    }

    public void Init()
    {
        ModuleInit();
        Reset();

        Command(0x36);
        Data(0x00);

        Command(0x3A);
        Data(0x05);

        Command(0xB2);
        Data(0x0C);
        Data(0x0C);
        Data(0x00);
        Data(0x33);
        Data(0x33);

        Command(0xB7);
        Data(0x35);

        Command(0xBB);
        Data(0x35);

        Command(0xC0);
        Data(0x2C);

        Command(0xC2);
        Data(0x01);

        Command(0xC3);
        Data(0x13);

        Command(0xC4);
        Data(0x20);

        Command(0xC6);
        Data(0x0F);

        Command(0xD0);
        Data(0xA4);
        Data(0xA1);

        Command(0xE0);
        Data(0xF0);
        Data(0xF0);
        Data(0x00);
        Data(0x04);
        Data(0x05);
        Data(0x29);
        Data(0x33);
        Data(0x3E);
        Data(0x38);
        Data(0x12);
        Data(0x12);
        Data(0x28);
        Data(0x30);

        Command(0xE1);
        Data(0xF0);
        Data(0x07);
        Data(0x0A);
        Data(0x0D);
        Data(0x0B);
        Data(0x07);
        Data(0x28);
        Data(0x33);
        Data(0x3E);
        Data(0x36);
        Data(0x14);
        Data(0x29);
        Data(0x32);

        Command(0x21);

        Command(0x11);

        Command(0x29);
    }

    public void SetWindows(int xStart, int yStart, int xEnd, int yEnd)
    {
        Command(0x2A);
        Data((byte)(((xStart) >> 8) & 0xff));
        Data((byte)((xStart + 34) & 0xff));
        Data((byte)((xEnd - 1 + 34) >> 8 & 0xff));
        Data((byte)((xEnd - 1 + 34) & 0xff));

        Command(0x2B);
        Data((byte)((yStart) >> 8 & 0xff));
        Data((byte)((yStart) & 0xff));
        Data((byte)((yEnd - 1) >> 8 & 0xff));
        Data((byte)((yEnd - 1) & 0xff));

        Command(0x2C);
    }

    public void ShowImage(Image<Bgr24> image, int xStart = 0, int yStart = 0)
    {
        int imwidth = image.Width;
        int imheight = image.Height;

        if (imwidth == Height && imheight == Width)
        {
            var pix = new byte[imheight * imwidth * 2];
            for (int y = 0; y < imheight; y++)
            {
                for (int x = 0; x < imwidth; x++)
                {
                    var color = image[x, y];
                    pix[(y * imwidth + x) * 2] = (byte)((color.R & 0xF8) | (color.G >> 5));
                    pix[(y * imwidth + x) * 2 + 1] = (byte)(((color.G << 3) & 0xE0) | (color.B >> 3));
                }
            }
            SetWindows(0, 0, Width, Height);
            DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
        }
        else
        {
          
        }
    }

    public void Clear()
    {
        var buffer = new byte[Width * Height * 2];
        Array.Fill(buffer, (byte)0xff);
        Thread.Sleep(20);
        SetWindows(0, 0, Width, Height);
        DigitalWrite(DC_PIN, true);
        for (int i = 0; i < buffer.Length; i += 4096)
        {
            SpiWriteByte(buffer.AsSpan(i, Math.Min(4096, buffer.Length - i)).ToArray());
        }
    }
}
