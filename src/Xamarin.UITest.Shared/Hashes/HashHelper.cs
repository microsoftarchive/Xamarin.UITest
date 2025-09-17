using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Xamarin.UITest.Shared.Hashes
{
    public class HashHelper
    {
        public string GetSha256Hash(DirectoryInfo directoryInfo)
        {
            return GetSha256Hash(directoryInfo.GetFiles("*", SearchOption.AllDirectories));
        }

        public string GetSha256Hash(byte[] bytes)
        {
            using (var cryptoProvider = SHA256.Create())
            {
                byte[] hashBytes = cryptoProvider.ComputeHash(bytes);

                var hexHash = BitConverter.ToString(hashBytes);
                return hexHash.Replace("-", string.Empty);
            }
        }

        public string GetSha256Hash(string str)
        {
            using (var cryptoProvider = SHA256.Create())
            {
                byte[] hashBytes = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(str));

                var hexHash = BitConverter.ToString(hashBytes);
                return hexHash.Replace("-", string.Empty);
            }
        }

        public string GetSha256Hash(string[] strs)
        {
            using (var cryptoProvider = SHA256.Create())
            {
                var hashBytes = new List<byte>();

                foreach (var str in strs)
                {
                    byte[] hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(str));
                    hashBytes.AddRange(hash);
                }

                var hexHash = BitConverter.ToString(cryptoProvider.ComputeHash(hashBytes.ToArray()));
                return hexHash.Replace("-", String.Empty);
            }
        }

        public string GetSha256Hash(FileInfo[] fileInfos)
        {
            if (fileInfos == null || !fileInfos.Any())
            {
                throw new ArgumentException("Must supply files for hashing.", nameof(fileInfos));
            }

            using (var cryptoProvider = SHA256.Create())
            {
                var hashBytes = new List<byte>();

                foreach (FileInfo file in fileInfos)
                {
                    if (!file.Exists)
                    {
                        throw new ArgumentException("File for hashing not found: " + file.Name, nameof(fileInfos));
                    }

                    using (var fileStream = file.OpenRead())
                    {
                        byte[] fileHash = cryptoProvider.ComputeHash(fileStream);
                        hashBytes.AddRange(fileHash);
                    }
                }

                var hexHash = BitConverter.ToString(cryptoProvider.ComputeHash(hashBytes.ToArray()));
                return hexHash.Replace("-", String.Empty);
            }
        }

        public string GetSha256Hash(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("File for hashing not found: " + fileInfo.Name, nameof(fileInfo));
            }

            using (var cryptoProvider = SHA256.Create())
            {
                using (var fileStream = fileInfo.OpenRead())
                {
                    byte[] fileHash = cryptoProvider.ComputeHash(fileStream);
                    var hexHash = BitConverter.ToString(fileHash);
                    return hexHash.Replace("-", String.Empty);
                }
            }
        }

        public string GetCombinedSha1Hash(object[] objects)
        {
            if (objects == null || !objects.Any())
            {
                return null;
            }

            var hashes = new List<string>();

            foreach (var o in objects)
            {
                if (o == null)
                {
                    continue;
                }

                var str = o as string;
                var file = o as FileInfo;
                var directory = o as DirectoryInfo;

                if (str != null)
                {
                    hashes.Add(GetSha256Hash(str));
                }
                else if (file != null)
                {
                    hashes.Add(GetSha256Hash(file));
                }
                else if (directory != null)
                {
                    hashes.Add(GetSha256Hash(directory));
                }
                else
                {
                    throw new Exception(string.Format("Unable to hash type: {0}", o.GetType().FullName));
                }
            }

            hashes.RemoveAll(x => x == null);

            if (!hashes.Any())
            {
                return null;
            }

            if (hashes.Count == 1)
            {
                return hashes.First();
            }

            return GetSha256Hash(hashes.ToArray());
        }

        public string GetStackTraceHash()
        {
            var stackTrace = new StackTrace();

            var lines = stackTrace.GetFrames()
                .Select(x => String.Format("{0}:{1}", x.GetMethod().Name, x.GetNativeOffset()));

            var str = String.Join(" -- ", lines);

            return GetSha256Hash(str);
        }
    }
}