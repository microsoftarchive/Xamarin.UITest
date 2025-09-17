using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Extensions;
using Xamarin.UITest.Shared.Hashes;

namespace Xamarin.UITest.Shared.Android
{
    public class TokenStorage
    {
        readonly IExecutor _executor;
        readonly ArtifactFolder _artifactFolder;

        public TokenStorage(IExecutor executor, ArtifactFolder artifactFolder)
        {
            _artifactFolder = artifactFolder;
            _executor = executor;
        }

        //TODO: Refactor this so that it returns a list of packages which need installing
        // Rather than checking the package is installed twice (once here and once in LocalAndroidAppLifeCycle)
        public bool HasMatchingTokens(string deviceSerial, params ApkFile[] apkFiles)
        {
            var installedPackages = _executor.Execute(new QueryAdbInstalledPackages(deviceSerial));

            foreach (var apkFile in apkFiles)
            {
                var package = installedPackages.FirstOrDefault(x => x.Package == apkFile.PackageName);

                // Once we discover a package is not installed, assume the rest
                // of the packages in the list are also not installed.
                if (package == null)
                {
                    return false;
                }

                var token = GetTokenSnapshot(deviceSerial, package);

                if (token.Exists(_artifactFolder))
                {
                    continue;
                }

                var sdkVersion = _executor.Execute(new QueryAdbSdkVersion(deviceSerial));
                var installedPackageSha256 = _executor.Execute(new QueryAdbInstalledPackageSha256(deviceSerial, package, sdkVersion));

                if (installedPackageSha256 == apkFile.GetSha256Hash())
                {
                    token.WriteToken(_artifactFolder);
                    continue;
                }

                return false;
            }

            return true;
        }

        public void SaveTokens(string deviceSerial, params ApkFile[] apkFiles)
        {
            var installedPackages = _executor.Execute(new QueryAdbInstalledPackages(deviceSerial));

            foreach (var apkFile in apkFiles)
            {
                var package = installedPackages.FirstOrDefault(x => x.Package == apkFile.PackageName);
                var token = GetTokenSnapshot(deviceSerial, package);

                if (token.Exists(_artifactFolder))
                {
                    continue;
                }

                token.WriteToken(_artifactFolder);
            }
        }

        IToken GetTokenSnapshot(string deviceSerial, InstalledPackage package)
        {
            var installedPackageLsLong = _executor.Execute(new QueryAdbInstalledPackageLsLong(deviceSerial, package));

            if (installedPackageLsLong.IsNullOrWhiteSpace())
            {
                return new NullToken();
            }

            return new Token(deviceSerial, package, installedPackageLsLong);
        }

        interface IToken
        {
            bool Exists(ArtifactFolder artifactFolder);
            void WriteToken(ArtifactFolder artifactFolder);
        }

        class Token : IToken
        {
            readonly HashHelper _hashHelper = new HashHelper();
            readonly string _sha1Token;
            readonly string _contents;

            public Token(string deviceSerial, InstalledPackage package, string lsLongOutput)
            {
                _sha1Token = _hashHelper.GetSha256Hash(new [] { deviceSerial, package.Package, lsLongOutput });

                _contents = string.Join(Environment.NewLine, new []
                {
                    string.Format("SHA-1 token: {0}", _sha1Token),
                    string.Format("Device serial: {0}", deviceSerial),
                    string.Format("Package name: {0}", package.Package),
                    string.Format("ls -l output: {0}", lsLongOutput),
                });
            }

            public bool Exists(ArtifactFolder artifactFolder)
            {
                return artifactFolder.HasArtifact(FileName);
            }

            public void WriteToken(ArtifactFolder artifactFolder)
            {
                artifactFolder.CreateArtifact(FileName, path => File.WriteAllText(path, _contents));
            }

            string FileName
            {
                get { return string.Format("token-{0}", _sha1Token); }
            }
        }

        class NullToken : IToken
        {
            public bool Exists(ArtifactFolder artifactFolder)
            {
                return false;
            }

            public void WriteToken(ArtifactFolder artifactFolder)
            {

            }
        }
    }
}