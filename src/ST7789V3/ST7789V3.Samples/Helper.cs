using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LedMatrix.Helpers
{
    static class Helper
    {
        private static string FILE_LOCATION = "/dev/fb1";
        public static void WriteToFile(byte[] contentToWrite)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(FILE_LOCATION, FileMode.Open)))
            {
                for (int i = 0; i < contentToWrite.Length; ++i)
                {
                    writer.Seek(i, SeekOrigin.Begin);
                    writer.Write(contentToWrite[i]);
                }
            }
        }

        public static byte[] ConvertToByteArray(this short[] source)
        {
            byte[] arrayAsByte = new byte[source.Length * 2/*sizeof short*/];
            Buffer.BlockCopy(source, 0, arrayAsByte, 0, arrayAsByte.Length);
            return arrayAsByte;
        }

        public static byte[] ImageToRgb565(this Image<Bgr565> original, int width, int height)
        {
            Int16[] pixelList = new Int16[width * height];

            int idx = 0; //each iteration increments this value
            for (int columnIdx = 0; columnIdx < original.Height; ++columnIdx)
            {
                for (int rowIdx = 0; rowIdx < original.Width; ++rowIdx)
                {
                    var imagePx = original[rowIdx, columnIdx].PackedValue;

                    var red = imagePx & 0xf800; //À¶É«

                    var blue = imagePx & 0x07e0; //ÂÌÉ«

                    var green = imagePx & 0x001f; //ºìÉ«



                    Console.WriteLine($"b:{red}g:{blue}r:{green}");

                    var data = CreatePixelFromRgb((byte)green, (byte)blue, (byte)red);

                    Console.WriteLine(data.ToString());
                    //Console.WriteLine($"x:{original[rowIdx, columnIdx].ToVector3().X}y:{original[rowIdx, columnIdx].ToVector3().Y}z:{original[rowIdx, columnIdx].ToVector3().Z}");
                    pixelList[idx++] = data;
                    //pixelList[idx++] = (Int16)imagePx;
                }
            }
            return pixelList.ConvertToByteArray();
        }

        public static Int16 CreatePixelFromRgb(byte r, byte g, byte b)
        {
            return (Int16)((r << 11) | (g << 5) | b);
        }

        public static void Rotate(this Image<Bgr565> original, int angle)
        {
            if (angle == 0) return;
            original.Mutate(x => x.Rotate(angle));
        }
    }
}