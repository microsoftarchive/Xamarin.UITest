using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Logging;
using Xamarin.UITest.Shared.Resources;

namespace Xamarin.UITest.Utils.SpecFlow
{
    static class SpecFlowIntegrator
    {
        public static void CheckForSpecFlowAndLoadIntegration(ArtifactFolder artifactFolder)
        {
            const string SpecFlowSharedArtifactName = "Xamarin.UITest.SpecFlow.Shared.dll";
            const string SpecFlowAssemblyName = "TechTalk.SpecFlow";

            if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name.Equals(SpecFlowAssemblyName)))
            {
                var assemblyName = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(e => e.GetName().Name.Equals(SpecFlowAssemblyName)).GetName();

                if (assemblyName == null)
                {
                    throw new Exception($"Unable to load Specflow Integration due to multiple assemblies named '{SpecFlowAssemblyName}'");
                }

                var specFlowMajorVersion = assemblyName.Version.Major;
                String artifactName;

                switch (specFlowMajorVersion)
                {
                    case 1:
                        artifactName = "Xamarin.UITest.SpecFlow.dll";
                        break;
                    case 2:
                        artifactName = "Xamarin.UITest.SpecFlow2.dll";
                        break;
                    case 3:
                        artifactName = "Xamarin.UITest.SpecFlow3.dll";
                        break;
                    default:
                        artifactName = "Xamarin.UITest.SpecFlow.dll";
                        break;
                }

                Log.Debug($"{nameof(SpecFlowIntegrator)} Specflow artifact name: {artifactName}.");
                LoadSpecFlowArtifactsIntoAssembly(artifactFolder, artifactName, SpecFlowSharedArtifactName);
            }
        }

        internal static void LoadSpecFlowArtifactsIntoAssembly(ArtifactFolder artifactFolder, params string[] artifactNames)
        {
            foreach (var artifactName in artifactNames)
            {
                var assemblyPath = artifactFolder.CreateArtifact(artifactName, path =>
                {
                    var resourceLoader = new EmbeddedResourceLoader();
                    var bytes = resourceLoader.GetEmbeddedResourceBytes(Assembly.GetExecutingAssembly(), artifactName);
                    File.WriteAllBytes(path, bytes);
                });
                Assembly.LoadFile(assemblyPath);
            }
        }
    }
}