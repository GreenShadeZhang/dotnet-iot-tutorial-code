using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Iot.Lcd;

public static class Helper
{
    public static byte[] GetImageBytes(Image<Bgr24> image)
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

    public static byte[] GetImage24Bytes(Image<Bgr24> image)
    {
        int imwidth = image.Width;
        int imheight = image.Height;
        byte[] pix = new byte[imwidth * imheight * 2];

        for (int y = 0; y < imheight; y++)
        {
            for (int x = 0; x < imwidth; x++)
            {
                Bgr24 pixel = image[x, y];
                int index = (y * imwidth + x) * 2;
                pix[index] = (byte)((pixel.B & 0xF8) | (pixel.G >> 5));
                pix[index + 1] = (byte)(((pixel.G << 3) & 0xE0) | (pixel.R >> 3));
            }
        }

        return pix;
    }
}
