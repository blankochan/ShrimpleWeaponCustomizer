using System.IO.Compression;
using ShrimpleWeaponCustomizer;
using MelonLoader;
using System.Text;
namespace ShrimpleWeaponCustomizer;

public static class Extensions
{
    public static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<T> ToIl2CppStructArray<T>(this T[] arr) where T : unmanaged
    {
        return new(arr);
    }
}
public static class CompressionUtils
{
    public static byte[] Compress(byte[] data)
    {
        using (var outputStream = new MemoryStream())
        {
            // Use BrotliStream in Compress mode
            using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.SmallestSize))
            {
                brotliStream.Write(data, 0, data.Length);
            }

            byte[] compressedData = outputStream.ToArray();
            if (compressedData.Length >= 3276)
            {
                Melon<Core>.Logger.Warning("Youre overriding too many weapons at once, you may experience bugs/desyncs (i am trying to fix it, its just complicated)");
            }
            return compressedData;
        }
    }

    public static byte[] Decompress(byte[] compressedData)
    {

        using (var inputStream = new MemoryStream(compressedData))
        using (var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            brotliStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }

}

