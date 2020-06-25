﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime;
using System.Threading.Tasks;

namespace PPeX
{
    public static class Utility
    {
        public static bool CompareBytes(byte[] arrayA, byte[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
                return false;

            for (int i = 0; i < arrayA.Length; i++)
                if (arrayA[i] != arrayB[i])
                    return false;

            return true;
        }

        public static Md5Hash GetMd5(ReadOnlySpan<byte> data)
        {
            using MD5 md5 = MD5.Create();

            byte[] md5Buffer = new byte[16];

            md5.TryComputeHash(data, md5Buffer, out _);

            return md5Buffer;
        }

        public static Md5Hash GetMd5(byte[] data)
        {
            using MD5 md5 = MD5.Create();
            return md5.ComputeHash(data);
        }

        public static Md5Hash GetMd5(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            return md5.ComputeHash(stream);
        }

        public static Md5Hash GetMd5(Stream stream, long offset, long length)
        {
            using MD5 md5 = MD5.Create();
            var sub = new PartialStream(stream, offset, length);

            return md5.ComputeHash(sub);
        }

        public static Task<Md5Hash> GetMd5Async(Stream stream, long offset, long length)
        {
            using var partialStream = new PartialStream(stream, offset, length);
            return GetMd5Async(partialStream);
        }

        public static async Task<Md5Hash> GetMd5Async(Stream stream)
        {
            using var md5 = MD5.Create();

            const int bufferSize = 16384;

            var buffer = new byte[bufferSize];


            while (true)
            {
	            var read = await stream.ReadAsync(buffer, 0, bufferSize);

	            if (read == 0)
		            break;

	            if (read == bufferSize)
	            {
		            md5.TransformBlock(buffer, 0, bufferSize, null, 0);
	            }
	            else
	            {
		            md5.TransformFinalBlock(buffer, 0, read);
		            break;
	            }
            }

            return md5.Hash;
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        public static void GCCompress()
        {
            //Collect garbage and compact the heap
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
    }

    public class ByteArrayComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            if (x.Length > y.Length)
                return 1;

            if (x.Length < y.Length)
                return -1;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] > y[i])
                    return 1;

                if (x[i] < y[i])
                    return -1;
            }

            return 0;
        }
    }

    public class ByteEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return Utility.CompareBytes(x, y);
        }

        public int GetHashCode(byte[] obj)
        {
            return obj.Sum(x => x.GetHashCode());
        }
    }
}
