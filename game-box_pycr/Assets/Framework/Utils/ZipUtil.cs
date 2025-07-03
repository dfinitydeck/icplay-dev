// using System.IO;
// using Ionic.Zlib;

namespace Framework
{
    public static class ZipUtil
    {
        // public static byte[] Compress(byte[] rawData)
        // {
        //     byte[] compressedData;
        //     using (MemoryStream ms = new MemoryStream())
        //     {
        //         using (ZlibStream zlibStream = new ZlibStream(ms, CompressionMode.Compress))
        //         {
        //             zlibStream.Write(rawData, 0, rawData.Length);
        //         }
        //         compressedData = ms.ToArray();
        //     }
        //     return compressedData;
        // }

        // public static byte[] Decompress(byte[] rawData)
        // {
        //     byte[] decompressedData;
        //     using (MemoryStream ms = new MemoryStream(rawData))
        //     {
        //         using (ZlibStream zlibStream = new ZlibStream(ms, CompressionMode.Decompress))
        //         {
        //             using (MemoryStream output = new MemoryStream())
        //             {
        //                 zlibStream.CopyTo(output);
        //                 decompressedData = output.ToArray();
        //             }
        //         }
        //     }
        //     return decompressedData;
        // }
    }
}
