namespace ColorTest
{
    public static class TGA
    {
        public static void Save(string path, Texture texture)
        {
            using FileStream fs = File.Create(path);
            using BinaryWriter bw = new(fs);

            // --- Header ---
            bw.Write((byte)0);               // id length
            bw.Write((byte)0);               // color map type
            bw.Write((byte)2);               // image type

            // Color map specification
            bw.Write((short)0);              // first entry index
            bw.Write((short)0);              // color map length
            bw.Write((byte)0);               // color map entry size

            // Image specification
            bw.Write((short)0);              // x-origin
            bw.Write((short)0);              // y-origin
            bw.Write((short)texture.Width);  // width
            bw.Write((short)texture.Height); // height
            bw.Write((byte)32);              // bits per pixel
            bw.Write((byte)0x28);            // descriptor

            for (int index = 0; index < texture.Width * texture.Height; index++)
            {
                texture.Pixels[index].ToRgba8(
                    out byte r,
                    out byte g,
                    out byte b,
                    out byte a);

                bw.Write(b);
                bw.Write(g);
                bw.Write(r);
                bw.Write(a);
            }
        }

        public static Texture Load(string path)
        {
            using FileStream fs = File.OpenRead(path);
            using BinaryReader br = new(fs);

            // --- Header ---
            byte idLength = br.ReadByte();
            byte colorMapType = br.ReadByte();
            byte imageType = br.ReadByte();

            // Only support uncompressed true-color
            if (imageType is not 2 and not 10)
                throw new NotSupportedException($"Unsupported TGA type. (got type {imageType})");

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
            bool originRight = (descriptor & 0x10) != 0;

            int size = width * height;
            byte r = 0, g = 0, b = 0, a = 255;

            if (imageType == 2)
            {
                for (int y = 0; y < height; y++)
                {
                    int dstY = originTop ? y : (height - 1 - y);
                    for (int x = 0; x < width; x++)
                    {
                        b = br.ReadByte();
                        g = br.ReadByte();
                        r = br.ReadByte();
                        a = (bytesPerPixel == 4) ? br.ReadByte() : (byte)255;

                        int dstX = originRight ? (width - 1 - x) : x;
                        texture[dstX, dstY] = Color.Rgba8(r, g, b, a);
                    }
                }

                return texture;
            }

            int index = 0;
            while (index < size)
            {
                byte header = br.ReadByte();
                bool isRle = (header & 0x80) != 0;
                int count = (header & 0x7F) + 1;

                if (isRle)
                {
                    b = br.ReadByte();
                    g = br.ReadByte();
                    r = br.ReadByte();
                    a = (bytesPerPixel == 4) ? br.ReadByte() : (byte)255;
                }

                for (int i = 0; i < count; i++)
                {
                    if (!isRle)
                    {
                        b = br.ReadByte();
                        g = br.ReadByte();
                        r = br.ReadByte();
                        a = (bytesPerPixel == 4) ? br.ReadByte() : (byte)255;
                    }

                    int x = index % width;
                    int y = index / width;

                    int dstX = originRight ? (width - 1 - x) : x;
                    int dstY = originTop ? y : (height - 1 - y);

                    texture[dstX, dstY] = Color.Rgba8(r, g, b, a);

                    index++;
                }
            }

            return texture;
        }
    }
}
