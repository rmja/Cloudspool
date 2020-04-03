using AdvancedStringBuilder;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Api.Generators.JavaScript
{
    public class ResourceScriptFactory
    {
        private readonly ObjectPool<StringBuilder> _builderPool;

        public ResourceScriptFactory(ObjectPoolProvider objectPoolProvider)
        {
            _builderPool = objectPoolProvider.CreateStringBuilderPool();
        }

        public string CreateFromExtension(byte[] resource, string extension) => extension switch
        {
            ".json" => CreateJson(resource),
            ".bin" => CreateBin(resource),
            ".bmp" => CreateBmp(resource),
            _ => null,
        };

        private static string CreateJson(byte[] resource)
        {
            var json = Encoding.UTF8.GetString(resource);

            return $"export default {json};";
        }

        private string CreateBin(byte[] resource)
        {
            var builder = _builderPool.Get();

            try
            {
                builder.EnsureCapacity(100 + 4 * resource.Length); // ',' and three digits per element
                builder.Append("const resource = new Uint8Array([");
                builder.AppendJoin(',', resource);
                builder.Append("]);");
                builder.Append("export default resource;");
                return builder.ToString();
            }
            finally
            {
                _builderPool.Return(builder);
            }
        }

        private string CreateBmp(byte[] resource)
        {
            using var stream = new MemoryStream(resource);
            using var bitmap = new Bitmap(stream);

            var size = bitmap.Width * bitmap.Height * 4;
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var builder = _builderPool.Get();

            try
            {
                Marshal.Copy(bitmapData.Scan0, buffer, 0, size);

                // Swap bytes from ARGB as defined by PixelFormat.Format32bppArgb to RGBA which is the format for ImageData

                if (BitConverter.IsLittleEndian)
                {
                    for (var offset = 0; offset < size; offset += 4)
                    {
                        // current: B, G, R, A, B, G, R, A, ...
                        // desired: R, G, B, A, R, G, B, A, ...

                        var B = buffer[offset + 0];
                        buffer[offset + 0] = buffer[offset + 2];
                        buffer[offset + 2] = B;
                    }
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                builder.EnsureCapacity(150 + 4 * size); // ',' and at most three digits per element
                builder.AppendLine("const f = 255;");
                builder.Append("const data = new Uint8ClampedArray([");
                AppendByte(builder, buffer[0]);
                for (var i = 1; i < size; i++)
                {
                    builder.Append(',');
                    AppendByte(builder, buffer[i]);
                }
                builder.AppendLine("]);");
                builder.AppendFormatLine("const resource = new ImageData(data, {0});", bitmap.Width);
                builder.Append("export default resource;");
                return builder.ToString();

                static void AppendByte(StringBuilder sb, byte b)
                {
                    if (b == 0xff)
                    {
                        sb.Append('f');
                    }
                    else
                    {
                        sb.Append(b);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
                ArrayPool<byte>.Shared.Return(buffer);
                _builderPool.Return(builder);
            }
        }
    }
}
