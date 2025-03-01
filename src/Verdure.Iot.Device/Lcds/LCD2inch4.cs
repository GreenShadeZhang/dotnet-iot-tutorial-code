using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Pwm.Drivers;
using System.Device.Spi;

namespace Verdure.Iot.Device;
public class LCD2inch4 : LcdConfig
{
    public const int Width = 240;
    public const int Height = 320;
    public LCD2inch4(SpiDevice spi, SoftwarePwmChannel pwmBacklight, int spiFreq = 40000000, int rst = 27, int dc = 25, int bl = 18, int blFreq = 1000) : base(spi, pwmBacklight, spiFreq, rst, dc, bl, blFreq)
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
        Command(0x36);
        Data(0x00);

        Command(0x3A);
        Data(0x05);

        Command(0x21);

        Command(0x2A);
        Data(0x00);
        Data(0x00);
        Data(0x01);
        Data(0x3F);

        Command(0x2B);
        Data(0x00);
        Data(0x00);
        Data(0x00);
        Data(0xEF);

        Command(0xB2);
        Data(0x0C);
        Data(0x0C);
        Data(0x00);
        Data(0x33);
        Data(0x33);

        Command(0xB7);
        Data(0x35);

        Command(0xBB);
        Data(0x1F);

        Command(0xC0);
        Data(0x2C);

        Command(0xC2);
        Data(0x01);

        Command(0xC3);
        Data(0x12);

        Command(0xC4);
        Data(0x20);

        Command(0xC6);
        Data(0x0F);

        Command(0xD0);
        Data(0xA4);
        Data(0xA1);

        Command(0xE0);
        Data(0xD0);
        Data(0x08);
        Data(0x11);
        Data(0x08);
        Data(0x0C);
        Data(0x15);
        Data(0x39);
        Data(0x33);
        Data(0x50);
        Data(0x36);
        Data(0x13);
        Data(0x14);
        Data(0x29);
        Data(0x2D);

        Command(0xE1);
        Data(0xD0);
        Data(0x08);
        Data(0x10);
        Data(0x08);
        Data(0x06);
        Data(0x06);
        Data(0x39);
        Data(0x44);
        Data(0x51);
        Data(0x0B);
        Data(0x16);
        Data(0x14);
        Data(0x2F);
        Data(0x31);

        Command(0x21);

        Command(0x11);

        Command(0x29);
    }

    public void SetWindows(int Xstart, int Ystart, int Xend, int Yend)
    {
        Command(0x2A);
        Data((byte)(Xstart >> 8));
        Data((byte)(Xstart & 0xff));
        Data((byte)(Xend >> 8));
        Data((byte)((Xend - 1) & 0xff));

        Command(0x2B);
        Data((byte)(Ystart >> 8));
        Data((byte)(Ystart & 0xff));
        Data((byte)(Yend >> 8));
        Data((byte)((Yend - 1) & 0xff));

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

            Command(0x36);
            Data(0x70);
            SetWindows(0, 0, Width, Height);
            DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
        }
        else
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

            Command(0x36);
            Data(0x00);
            SetWindows(0, 0, Width, Height);
            DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
        }
    }

    public void ShowImageData(Image<Bgr24> image, int xStart = 0, int yStart = 0)
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

            //DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
        }
        else
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

            //DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
        }
    }

    public byte[] GetImageBytes(Image<Bgr24> image, int xStart = 0, int yStart = 0)
    {
        int imwidth = image.Width;
        int imheight = image.Height;
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
        return pix;
    }

    public void ShowImageBytes(byte[] pix)
    {
        //SetWindows(0, 0, Width, Height);
        //DigitalWrite(DC_PIN, true);
        for (int i = 0; i < pix.Length; i += 4096)
        {
            SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
        }
    }

    public void Clear()
    {
        var buffer = new byte[Width * Height * 2];
        Array.Fill(buffer, (byte)(0xff & 0xF800));
        //Thread.Sleep(20);
        //SetWindows(0, 0, Width, Height);
        DigitalWrite(DC_PIN, true);
        for (int i = 0; i < buffer.Length; i += 4096)
        {
            SpiWriteByte(buffer.AsSpan(i, Math.Min(4096, buffer.Length - i)).ToArray());
        }
    }

    public void ClearColor(ushort color)
    {
        var buffer = new byte[Width * Height * 2];
        for (int i = 0; i < buffer.Length; i += 2)
        {
            buffer[i] = (byte)(color >> 8);
            buffer[i + 1] = (byte)(color & 0xff);
        }
        Thread.Sleep(20);
        SetWindows(0, 0, Width, Height);
        DigitalWrite(DC_PIN, true);
        for (int i = 0; i < buffer.Length; i += 4096)
        {
            SpiWriteByte(buffer.AsSpan(i, Math.Min(4096, buffer.Length - i)).ToArray());
        }
    }
}
