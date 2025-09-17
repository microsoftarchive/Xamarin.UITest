using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Hashes;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Zip;
using System.Threading;

namespace Xamarin.UITest.Shared.Android
{
    public class ApkFile : IApkFileInformation
    {
        readonly string _path;
        readonly IExecutor _executor;
        readonly HashHelper _hashHelper = new HashHelper();
        readonly Lazy<AaptDumpResult> _aaptDump;

        public ApkFile(string path, IExecutor executor)
        {
            _path = path;
            _executor = executor;

            _aaptDump = new Lazy<AaptDumpResult>(GetAaptDump);
        }

        public string ApkPath
        {
            get { return _path; }
        }

        public string[] GetFingerprints(ArtifactFolder artifactFolder)
        {
            var sbfFilePath = artifactFolder.CreateArtifact(string.Format("{0}.sbf", GetSha256Hash()), ExtractSignatureBlockFile);

            return _executor.Execute(new QueryRsaFileFingerprints(sbfFilePath));
        }

        public void ExtractSignatureBlockFile(string path)
        {
            using (var stream = File.OpenRead(_path))
            {
                using (var archive = new ZipFile(stream))
                {
                    ZipEntry[] signingEntries = archive
                        .OfType<ZipEntry>()
                        .Where(x => x.Name.StartsWithIgnoreCase("META-INF"))
                        .Where(x => x.Name.EndsWithIgnoreCase(".rsa") || x.Name.EndsWithIgnoreCase(".dsa") || x.Name.EndsWithIgnoreCase(".ec"))
                        .ToArray();

                    if (signingEntries.Length == 0)
                    {
                        throw new Exception(string.Format("{0} does not appear to have been signed.", _path));
                    } 
                    else if (signingEntries.Length != 1)
                    {
                        throw new Exception(string.Format("{0} appears to have been signed more that once", _path));
                    }
                    ZipEntry rsaEntry = signingEntries.Single();
                    ExtractEntry(archive, rsaEntry, path);
                }
            }
        }

        public void ExtractSigningInfo(string path)
        {
            var buffer = new byte[4096];
            using (var inStream = File.OpenRead(_path))
            {
                using (var archive = new ZipFile(inStream))
                {
                    using (var outSteam = File.Create(path))
                    {
                        using (var outArchive = new ZipOutputStream(outSteam))
                        {
                            ZipEntry[] siEntries = archive.OfType<ZipEntry>().Where(x => IsSigningInfo(x.Name)).ToArray();
                            foreach (var inputEntry in siEntries)
                            {
                                var inputStream = archive.GetInputStream(inputEntry);
                                var outputEntry = new ZipEntry(inputEntry.Name);
                                outputEntry.Size = inputEntry.Size;
                                outputEntry.DateTime = inputEntry.DateTime;
                                outArchive.PutNextEntry(outputEntry);
                                StreamUtils.Copy(inputStream, outArchive, buffer);
                                outArchive.CloseEntry();
                            }
                        }
                    }
                }
            }
        }

        bool IsSigningInfo(string name)
        {
            return name.StartsWithIgnoreCase("META-INF") && (
                name.EndsWithIgnoreCase(".rsa") ||
                name.EndsWithIgnoreCase(".sf") ||
                name.EndsWithIgnoreCase("manifest.mf")); 
        }

        public ApkFile InjectSigningInfo(FileInfo signingInfoFile, string path)
        {
            File.Copy(_path, path);

            using (var signingInfo = signingInfoFile.OpenRead())
            {
                using (var archive = new ZipFile(signingInfo))
                {
                    using (var outFile = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        using (var outArchive = new ZipFile(outFile))
                        {
                            ZipEntry[] siEntries = archive.OfType<ZipEntry>().Where(x => IsSigningInfo(x.Name)).ToArray();
                            outArchive.BeginUpdate();
                            foreach (var inputEntry in siEntries)
                            {
                                var bytes = new byte[inputEntry.Size];
                                var inputSteam = archive.GetInputStream(inputEntry);
                                inputSteam.Read(bytes, 0, bytes.Length);
                                // Cannot use using for dataSource, CommitUpdate fails 
                                var dataSource = new ByteArrayDataSource(bytes);
                                outArchive.Add(dataSource, inputEntry.Name);
                            }
                            outArchive.CommitUpdate();
                        }
                    }
                }
            }

            return new ApkFile(path, _executor);
        }

        public string PackageName
        {
            get { return _aaptDump.Value.PackageName; }
        }

        public List<string> Permissions
        {
            get { return _aaptDump.Value.Permissions; }
        }
        
        AaptDumpResult GetAaptDump()
        {
            var maxAttempts = 5;

            for (var i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    return _executor.Execute(new QueryAaptDumpXmltreeManifest(this));
                }
                catch (Exception treeEx)
                {
                    try
                    {
                        return _executor.Execute(new QueryAaptDumpBadging(this));
                    }
                    catch (Exception badgingEx)
                    {
                        if (i == maxAttempts)
                        {
                            throw new Exception(string.Concat(
                                $"Error querying {ApkPath}", Environment.NewLine,
                                Environment.NewLine,
                                "aapt dump xmltree:", Environment.NewLine,
                                treeEx.Message, Environment.NewLine,
                                Environment.NewLine,
                                "aapt dump badging:", Environment.NewLine,
                                badgingEx.Message
                            ));
                        }
                    }
                }

                Thread.Sleep(1000);
            }

            throw new Exception($"Error querying {ApkPath}");
        }

        public bool HasInternetPermission()
        {
            return Permissions.Contains("android.permission.INTERNET");
        }

        public void EnsureInternetPermission()
        {
            if (!HasInternetPermission())
            {
                throw new Exception("App does not contain permission 'android.permission.INTERNET', please include this permission");
            }
        }

        public bool IsMissingDotNetAssemblies()
        {
            using (var stream = File.OpenRead(_path))
            {
                using (var archive = new ZipFile(stream))
                {
                    var entries = archive.OfType<ZipEntry>().ToArray();
                    
                    var monodroid = entries.Any(x => x.Name.EndsWith("libmonodroid.so"));
                    var hasRuntime = entries.Any(x => x.Name.EndsWith("mscorlib.dll") || x.Name.EndsWith("System.Private.CoreLib.dll"));
                    var hasEnterpriseBundle = entries.Any(x => x.Name.EndsWith("libmonodroid_bundle_app.so"));
                    var hasAssemblyStore = entries.Any(x => x.Name.EndsWith("assemblies.blob"));

                    return monodroid && !hasRuntime && !hasEnterpriseBundle && !hasAssemblyStore;
               }
            }
        }

        public void EnsureDotNetAssembliesAreBundled()
        {
            if (IsMissingDotNetAssemblies())
            {
                throw new Exception("No .NET assemblies were found in the application. Please disable Fast Deployment in the Visual Studio project property pages or edit the project file in a text editor and set the 'EmbedAssembliesIntoApk' MSBuild property to 'true'.");

            }
        }

        public string GetSha256Hash()
        {
            return _hashHelper.GetSha256Hash(new FileInfo(ApkPath));
        }

        void ExtractEntry(ZipFile zipFile, ZipEntry entry, string targetPath)
        {
            using (var inputStream = zipFile.GetInputStream(entry))
            {
                using (var outputStream = File.OpenWrite(targetPath))
                {
                    inputStream.CopyTo(outputStream);
                }
            }
        }

        public byte[] GetFileBytes(string fileName)
        {
            using (var stream = File.OpenRead(_path))
            {
                using (var archive = new ZipFile(stream))
                {
                    ZipEntry entry = archive
                        .OfType<ZipEntry>()
                        .Single(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));

                    using (Stream entryStream = archive.GetInputStream(entry))
                    {
                        using (var outputStream = new MemoryStream())
                        {
                            entryStream.CopyTo(outputStream);
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }

        public void AddFile(string fileName, byte[] fileBytes)
        {
            using (var stream = File.Open(_path, FileMode.Open))
            {
                using (var dataSource = new ByteArrayDataSource(fileBytes))
                {
                    using (var archive = new ZipFile(stream))
                    {
                        archive.BeginUpdate();
                        archive.Add(dataSource, fileName);
                        archive.CommitUpdate();
                    }
                }
            }
        }
    }
}
