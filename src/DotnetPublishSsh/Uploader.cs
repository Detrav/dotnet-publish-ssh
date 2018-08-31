using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Renci.SshNet;

namespace DotnetPublishSsh
{
    internal sealed class Uploader
    {
        public char DirectorySeparator { get; set; } = '/';

        private readonly List<FileHash> hashes = new List<FileHash>();
        

        private readonly ConnectionInfo _connectionInfo;
        private readonly HashSet<string> _existingDirectories = new HashSet<string>();

        public Uploader(PublishSshOptions publishSshOptions)
        {
            _connectionInfo = CreateConnectionInfo(publishSshOptions);
        }

        internal static ConnectionInfo CreateConnectionInfo(PublishSshOptions options)
        {
            var authenticationMethods = new List<AuthenticationMethod>();

            if (options.Password != null)
                authenticationMethods.Add(
                    new PasswordAuthenticationMethod(options.User, options.Password));

            if (options.KeyFile != null)
                authenticationMethods.Add(
                    new PrivateKeyAuthenticationMethod(options.User, new PrivateKeyFile(options.KeyFile)));

            var connectionInfo = new ConnectionInfo(
                options.Host,
                options.Port,
                options.User,
                authenticationMethods.ToArray());

            return connectionInfo;
        }

        public void UploadFiles(string path, ICollection<LocalFile> localFiles)
        {
            int count = 0;
            //using (var client = new SshClient(_connectionInfo))
            using (var ftp = new SftpClient(_connectionInfo))
            {
                //client.Connect();
                ftp.Connect();

                foreach (var localFile in localFiles)
                {
                    AddHash(localFile);
                }

                List<FileHash> existHashes = DownloadHashes(ftp, path);
                
                foreach (var localFile in localFiles)
                {
                    if (!EqualsHashes(localFile.RelativeName, hashes, existHashes))
                    {
                        count++;
                        UploadFile(localFile, ftp, path);
                    }
                    else
                        Console.WriteLine($"The file {localFile.RelativeName} are the same and will be skipped");
                }

                UploadNewHashes(ftp, path);
            }
            Console.WriteLine($"Uploaded {count} files.");
        }

        private bool EqualsHashes(string relativeName, List<FileHash> hashes, List<FileHash> existHashes)
        {
            var h1 = hashes.FirstOrDefault(p => p.Path == relativeName);
            var h2 = existHashes.FirstOrDefault(p => p.Path == relativeName);
            return h1 != null && h2 != null && h1.Lenght == h2.Lenght && h1.Path == h2.Path && h1.CreatedAt == h2.CreatedAt;
        }

        private List<FileHash> DownloadHashes(SftpClient ftp, string path)
        {
            List<FileHash> result = new List<FileHash>();
            var fullPath = path + ".hashes";
            if (ftp.Exists(fullPath))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ftp.DownloadFile(fullPath, ms);
                    ms.Position = 0;
                    XmlSerializer ser = new XmlSerializer(typeof(FileHash[]));
                    result.AddRange(ser.Deserialize(ms) as FileHash[]);
                }
            }
            return result;
        }

        private void UploadNewHashes(SftpClient ftp, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(FileHash[]));

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms))
                {
                    ser.Serialize(writer, hashes.ToArray());
                }

                ms.Position = 0;

                UploadStream(ms, ftp, path,  ".hashes");
            }
        }

        private void AddHash(LocalFile localFile)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                using (Stream stream = File.OpenRead(localFile.FileName))
                {
                    FileHash fileHash = new FileHash();
                    fileHash.Path = localFile.RelativeName;
                    fileHash.Lenght = stream.Length;
                    var hash = sha1.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);
                    fileHash.CreatedAt = File.GetLastWriteTimeUtc(localFile.FileName);
                    foreach (byte b in hash)
                    {
                        // can be "x2" if you want lowercase
                        sb.Append(b.ToString("X2"));
                    }
                    fileHash.Hash = sb.ToString();
                    hashes.Add(fileHash);
                }
            }
        }

        private void UploadFile(LocalFile localFile, SftpClient ftp, string path)
        {
            Console.WriteLine($"Uploading {localFile.RelativeName}");

            using (var stream = File.OpenRead(localFile.FileName))
            {
                var filePath = localFile.RelativeName.Replace(Path.DirectorySeparatorChar, DirectorySeparator);
                UploadStream(stream, ftp, path, filePath);
            }
        }

        private void UploadStream(Stream stream, SftpClient ftp, string path, string filePath)
        {
            var fullPath = path + filePath;
            EnsureDirExists(ftp, fullPath);
            ftp.UploadFile(stream, fullPath, true);
        }

        

        private void EnsureDirExists(SftpClient ftp, string path)
        {
            var parts = path.Split(new[] {DirectorySeparator}, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (!path.EndsWith(DirectorySeparator.ToString()))
                parts = parts.Take(parts.Count - 1).ToList();

            CreateDir(ftp, parts);
        }

        private void CreateDir(SftpClient ftp, ICollection<string> parts, bool noCheck = false)
        {
            if (parts.Any())
            {
                var path = Combine(parts);
                var parent = parts.Take(parts.Count - 1).ToList();

                if (noCheck || ftp.Exists(path))
                {
                    CreateDir(ftp, parent, true);
                }
                else
                {
                    CreateDir(ftp, parent);
                    ftp.CreateDirectory(path);
                }

                _existingDirectories.Add(path);
            }
        }

        private string Combine(ICollection<string> parts)
        {
            var path = DirectorySeparator +
                       string.Join(DirectorySeparator.ToString(), parts) +
                       (parts.Any() ? DirectorySeparator.ToString() : "");
            return path;
        }
    }
}