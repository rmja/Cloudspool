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

            return $"const resource = {json}; export default resource";
        }

        private string CreateBin(byte[] resource)
        {
            var builder = _builderPool.Get();

            try
            {
                builder.EnsureCapacity(100 + 4 * resource.Length); // ',' and three digits per element
                builder.Append("const resource = new Uint8Array([");
                builder.AppendJoin(',', resource);
                builder.Append("]);export default resource;");
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
            var ARGB = ArrayPool<byte>.Shared.Rent(size);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var builder = _builderPool.Get();

            try
            {
                Marshal.Copy(bitmapData.Scan0, ARGB, 0, size);

                builder.EnsureCapacity(150 + 4 * size); // ',' and at most three digits per element
                builder.Append("const data = new Uint8ClampedArray([");
                for (var offset = 0; offset < size; offset += 4)
                {
                    var rgba = BitConverter.ToUInt32(ARGB, offset);

                    builder.Append((rgba >> 16) & 0xFF); // R
                    builder.Append(',');
                    builder.Append((rgba >> 08) & 0xFF); // G
                    builder.Append(',');
                    builder.Append((rgba >> 00) & 0xFF); // B
                    builder.Append(',');
                    builder.Append((rgba >> 24) & 0xFF); // A
                    builder.Append(',');
                }
                builder.AppendFormat("]);const resource = new ImageData(data, {0});export default resource;", bitmap.Width);
                var script = builder.ToString();
                return script;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
                ArrayPool<byte>.Shared.Return(ARGB);
                _builderPool.Return(builder);
            }
        }
    }
}
