using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Spi;

namespace Iot.Lcd;
public class LCD2inch4 : LcdConfig
{
    public const int Width = 240;
    public const int Height = 320;

    private readonly SpiDevice _spi;
    public LCD2inch4(SpiDevice spi) : base(spi, 40000000, 27, 25, 18, 1000)
    {
        _spi = spi;
    }
    public void Command(byte cmd)
    {
        DigitalWrite(DC_PIN, false);
        SpiWriteByte(new byte[] { cmd });
    }

    public void Data(byte val)
    {
        DigitalWrite(DC_PIN, true);
        SpiWriteByte(new byte[] { val });
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

        Command(0x11); // Sleep out

        Command(0xCF);
        Data(0x00);
        Data(0xC1);
        Data(0x30);
        Command(0xED);
        Data(0x64);
        Data(0x03);
        Data(0x12);
        Data(0x81);
        Command(0xE8);
        Data(0x85);
        Data(0x00);
        Data(0x79);
        Command(0xCB);
        Data(0x39);
        Data(0x2C);
        Data(0x00);
        Data(0x34);
        Data(0x02);
        Command(0xF7);
        Data(0x20);
        Command(0xEA);
        Data(0x00);
        Data(0x00);
        Command(0xC0); // Power control
        Data(0x1D); // VRH[5:0]
        Command(0xC1); // Power control
        Data(0x12); // SAP[2:0]; BT[3:0]
        Command(0xC5); // VCM control
        Data(0x33);
        Data(0x3F);
        Command(0xC7); // VCM control
        Data(0x92);
        Command(0x3A); // Memory Access Control
        Data(0x55);
        Command(0x36); // Memory Access Control
        Data(0x08);
        Command(0xB1);
        Data(0x00);
        Data(0x12);
        Command(0xB6); // Display Function Control
        Data(0x0A);
        Data(0xA2);

        Command(0x44);
        Data(0x02);

        Command(0xF2); // 3Gamma Function Disable
        Data(0x00);
        Command(0x26); // Gamma curve selected
        Data(0x01);
        Command(0xE0); // Set Gamma
        Data(0x0F);
        Data(0x22);
        Data(0x1C);
        Data(0x1B);
        Data(0x08);
        Data(0x0F);
        Data(0x48);
        Data(0xB8);
        Data(0x34);
        Data(0x05);
        Data(0x0C);
        Data(0x09);
        Data(0x0F);
        Data(0x07);
        Data(0x00);
        Command(0xE1); // Set Gamma
        Data(0x00);
        Data(0x23);
        Data(0x24);
        Data(0x07);
        Data(0x10);
        Data(0x07);
        Data(0x38);
        Data(0x47);
        Data(0x4B);
        Data(0x0A);
        Data(0x13);
        Data(0x06);
        Data(0x30);
        Data(0x38);
        Data(0x0F);
        Command(0x29); // Display on
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
            Data(0x78);
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
            Data(0x08);
            SetWindows(0, 0, Width, Height);
            DigitalWrite(DC_PIN, true);
            for (int i = 0; i < pix.Length; i += 4096)
            {
                SpiWriteByte(pix.AsSpan(i, Math.Min(4096, pix.Length - i)).ToArray());
            }
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
