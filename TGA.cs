namespace ColorTest
{
    public static class TGA
    {
        public static Texture Load(string path)
        {
            using FileStream fs = File.OpenRead(path);
            using BinaryReader br = new(fs);

            // --- Header ---
            byte idLength = br.ReadByte();
            byte colorMapType = br.ReadByte();
            byte imageType = br.ReadByte();

            // Only support uncompressed true-color
            if (imageType != 2)
                throw new NotSupportedException("Only uncompressed true-color TGA (type 2) is supported.");

            // Color map specification (ignored)
            br.ReadInt16(); // first entry index
            br.ReadInt16(); // color map length
            br.ReadByte();  // color map entry size

            // Image specification
            br.ReadInt16(); // x-origin
            br.ReadInt16(); // y-origin
            int width = br.ReadInt16();
            int height = br.ReadInt16();
            byte bitsPerPixel = br.ReadByte();
            byte descriptor = br.ReadByte();

            if (colorMapType != 0)
                throw new NotSupportedException("Color-mapped TGA not supported.");

            if (bitsPerPixel is not 24 and not 32)
                throw new NotSupportedException("Only 24-bit and 32-bit TGA images are supported.");

            // Skip image ID field if present
            if (idLength > 0)
                br.ReadBytes(idLength);

            Texture texture = new(width, height);

            int bytesPerPixel = bitsPerPixel / 8;

            // Bit 5 (0x20) = top-left origin if set
            bool originTop = (descriptor & 0x20) != 0;

            // --- Read pixels ---
            for (int y = 0; y < height; y++)
            {
                int dstY = originTop ? y : (height - 1 - y);

                for (int x = 0; x < width; x++)
                {
                    byte b = br.ReadByte();
                    byte g = br.ReadByte();
                    byte r = br.ReadByte();
                    byte a = (bytesPerPixel == 4) ? br.ReadByte() : (byte)255;

                    texture[x, dstY] = Color.Rgba8(r, g, b, a);
                }
            }

            return texture;
        }
    }
}
