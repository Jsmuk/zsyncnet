using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Flurl.Http;
using zsyncnet.Internal;
using zsyncnet.Internal.ControlFile;

namespace zsyncnet
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Zsync
    {
        private static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Syncs a file
        /// </summary>
        /// <param name="zsyncFile"></param>
        /// <param name="output"></param>
        /// <returns>Number of bytes downloaded</returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        public static long Sync(Uri zsyncFile, DirectoryInfo output)
        {
            // Load zsync control file

            var cf = new ControlFile(zsyncFile.ToString().GetStreamAsync().Result);
            var path = Path.Combine(output.FullName, cf.GetHeader().Filename.TrimStart());

            Uri fileUri;

            if (cf.GetHeader().Url == null || !IsAbsoluteUrl(cf.GetHeader().Url))
            {
                // Relative
                fileUri = new Uri(zsyncFile.ToString().Replace(".zsync", string.Empty));
            }
            else
            {
                fileUri = new Uri(cf.GetHeader().Url);
            }

            if (fileUri.ToString().HeadAsync().Result.StatusCode == HttpStatusCode.NotFound)
            {
                // File not found
                throw new WebException("File not found");
            }

            if (File.Exists(path))
            {
                // File exists, use the existing file as the seed file

                return Sync(cf, new FileInfo(path), fileUri);
            }
            else
            {
                fileUri.ToString().DownloadFileAsync(output.FullName, cf.GetHeader().Filename).Wait();
                return cf.GetHeader().Length;

            }
        }

        public static long Sync(ControlFile controlFile, FileInfo localFile, Uri remoteFile)
        {
            var of = new OutputFile(localFile, controlFile, remoteFile);

            of.Patch();

            if (VerifyFile(of.TempPath, controlFile.GetHeader().Sha1))
            {
                File.Copy(of.TempPath.FullName,of.FilePath.FullName,true);
                File.Delete(of.TempPath.FullName);
            }
            else
            {
                throw new Exception("File invalid");
            }

            return of.TotalBytesDownloaded;
        }

        private static bool VerifyFile(FileInfo file, string checksum)
        {
            using var crypto = new SHA1CryptoServiceProvider();
            var fileStream = file.OpenRead();
            var hash = ZsyncUtil.ByteToHex(crypto.ComputeHash(fileStream));

            return hash == checksum;
        }

    }
}
