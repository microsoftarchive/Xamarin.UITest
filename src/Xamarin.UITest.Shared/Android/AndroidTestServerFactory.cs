using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Shared.Android.Commands;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Resources;

namespace Xamarin.UITest.Shared.Android
{
    public class AndroidTestServerFactory
    {
        readonly IExecutor _executor;

        public AndroidTestServerFactory(IExecutor executor)
        {
            _executor = executor;
        }

        public ApkFile BuildTestServer(
            ApkFile appApkFile, 
            KeyStore keyStore,
            ArtifactFolder artifactFolder,
            Assembly sharedAssembly = null)
        {
            string[] appFingerprints = appApkFile.GetFingerprints(artifactFolder);

            if (keyStore.Fingerprints.Any(f => !appFingerprints.Contains(f)))
            {
                throw new Exception("Fingerprints didn't match.");
            }

            return BuildTestServer(appApkFile.PackageName, keyStore, artifactFolder, sharedAssembly);
        }

        public ApkFile BuildTestServer(
            string packageName,
            KeyStore keyStore,
            ArtifactFolder artifactFolder,
            Assembly sharedAssembly = null)
        {
            sharedAssembly = sharedAssembly ?? typeof(AndroidTestServerFactory).Assembly;

            var signedTestServerApkPath = artifactFolder.CreateArtifact("SignedTestServer.apk", p =>
                {
                    Log.Debug("KeyStore: " + keyStore);

                    var testServerApkFile = BuildTestServerApkFile(sharedAssembly, packageName, artifactFolder);
                    keyStore.SignApk(testServerApkFile, p);
                });

            Log.Debug("Signed test server apk: " + signedTestServerApkPath);
            return new ApkFile(signedTestServerApkPath, _executor);
        }

        public ApkFile BuildTestServerWithSi(
            string appPackageName,
            FileInfo signingInfoFile,
            ArtifactFolder artifactFolder,
            Action<ApkFile> optionalValidation = null,
            Assembly sharedAssembly = null)
        {
            sharedAssembly = sharedAssembly ?? typeof(AndroidTestServerFactory).Assembly;

            var testServerApkFileName = artifactFolder.CreateArtifact("SignedTestServer.apk", path =>
                {
                var testServerApkFile = BuildTestServerApkFile(sharedAssembly, appPackageName, artifactFolder);

                    var signedApkFile = testServerApkFile.InjectSigningInfo(signingInfoFile, path);

                    validateApk(signedApkFile, "Cannot generate signed test server using signing info file. Try regenerating the signing info file using the command tool");

                    if(optionalValidation != null) 
                    {
                        optionalValidation.Invoke(signedApkFile);
                    }
                });

             return new ApkFile(testServerApkFileName, _executor);
        }

        public ApkFile BuildTestServerWithSi(
            ApkFile appApkFile,
            FileInfo signingInfoFile,
            ArtifactFolder artifactFolder,
            Assembly sharedAssembly = null)
        {
            validateApk(appApkFile, string.Format("Apk seems to have been modified after signing: {0}", appApkFile.ApkPath));

            Action<ApkFile> ensureSameCertificate = serverApk =>
            {
                // apk should be signed with same cert that generated the SI file

                var appFingerprints = appApkFile.GetFingerprints(artifactFolder);
                var siFingerprints = serverApk.GetFingerprints(artifactFolder);

                if (appFingerprints.Any(f => !siFingerprints.Contains(f)))
                {
                    throw new Exception("Apk is not signed with same certificate that was used for creating signing info file");
                }
            };

            return BuildTestServerWithSi(
                appApkFile.PackageName,
                signingInfoFile,
                artifactFolder,
                ensureSameCertificate,
                sharedAssembly); 
        }

        ApkFile BuildTestServerApkFile(
            Assembly sharedAssembly,
            string appPackageName,
            ArtifactFolder artifactFolder)
        {
            var resourceFinder = new EmbeddedResourceLoader();

            var manifestFileName = artifactFolder.CreateArtifact("AndroidManifest.xml", path =>
            {
                var manifestTemplate = resourceFinder.GetEmbeddedResourceString(
                    sharedAssembly,
                    "AndroidManifest.xml");

                string testServerManifest = manifestTemplate
                .Replace("#targetPackage#", appPackageName)
                .Replace("#testPackage#", string.Format("{0}.test", appPackageName));

                File.WriteAllText(path, testServerManifest);
            });

            Log.Debug("Manifest: " + manifestFileName);

            var dummyPackageFileName = artifactFolder.CreateArtifact("dummy.apk", path => _executor.Execute(new CommandAaptPackage(new FileInfo(manifestFileName), path)));

            var dummyApkFile = new ApkFile(dummyPackageFileName, _executor);
            Log.Debug("Dummy package: " + dummyPackageFileName);

            var testServerApkFileName = artifactFolder.CreateArtifact("TestServer.apk", path =>
                {
                    File.WriteAllBytes(path, resourceFinder.GetEmbeddedResourceBytes(sharedAssembly, "TestServer.apk"));

                    var serverApkFile = new ApkFile(path, _executor);

                    var manifestBytes = dummyApkFile.GetFileBytes("AndroidManifest.xml");
                    serverApkFile.AddFile("AndroidManifest.xml", manifestBytes);
                    Log.Debug("Added manifest to test server apk.");
                });

            var testServerApkFile = new ApkFile(testServerApkFileName, _executor);
            Log.Debug("Test server apk: " + testServerApkFileName);

            return testServerApkFile;
        }


        private void validateApk(ApkFile apkFile, string errorMessage)
        {
            try
            {
                _executor.Execute(new CommandApkSignerVerify(apkFile.ApkPath));

            }
            catch (Exception e)
            {
                if (e.Message.Contains("not correctly signed"))
                {
                    throw new Exception(errorMessage, e);
                }
                throw;
            }
        }
    }
}