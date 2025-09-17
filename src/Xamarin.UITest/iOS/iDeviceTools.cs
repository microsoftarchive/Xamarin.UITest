using System;
using System.IO;
using System.Reflection;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.Shared.Zip;

namespace Xamarin.UITest.iOS
{
    internal class iDeviceTools
    {
        readonly IProcessRunner _processRunner;
        readonly iProxy _iProxy;
        readonly iAppData _iAppData;

        public iDeviceTools(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
            var artifactFolder = new ArtifactFolder();
            var toolPath = artifactFolder.CreateArtifactFolder("idevice-tools", dir =>
            {
                var resourceLoader = new EmbeddedResourceLoader();
                var zipFile = Path.Combine(dir, "idevice-tools.zip");
                File.WriteAllBytes(zipFile, resourceLoader.GetEmbeddedResourceBytes(Assembly.GetExecutingAssembly(), "idevice-tools.zip"));

                ZipHelper.Unzip(zipFile, dir);
                File.Delete(zipFile);

                MakeExecutable(iAppData.CommandName, dir);
                MakeExecutable(iProxy.CommandName, dir);
            },
                "iproxy", "iappdata");

            _iProxy = new iProxy(_processRunner, toolPath);
            _iAppData = new iAppData(_processRunner, toolPath);
        }

        void MakeExecutable(string command, string dir)
        {
            var file = new FileInfo(Path.Combine(dir, command));

            if (!file.Exists)
            {
                throw new Exception(string.Format("Failed to make '{0}' executable - not able to find: {1}", command, file.FullName));
            }

            _processRunner.RunCommand("chmod", string.Join(" ", "a+x", file.FullName));
        }

        public iProxy iProxy
        {
            get { return _iProxy; }
        }

        public iAppData iAppData
        {
            get { return _iAppData; }
        }
    }
}
