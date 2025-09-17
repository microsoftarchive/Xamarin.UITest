using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Utils;
using Xamarin.UITest.Utils.SpecFlow;

namespace Xamarin.UITest.Tests.Specflow
{
    public class SpecflowIntegrationUnitTests
    {
        [TestCase("Xamarin.UITest.SpecFlow2")]
        public void LoadsEmbeddedArtifactsIntoTheAppDomain(string specflowAssemblyName)
        {
            var assembliesBefore = AppDomain.CurrentDomain.GetAssemblies().Select(e => e.GetName().Name);

            var artifactFolder = new ArtifactFolder();
            SpecFlowIntegrator.LoadSpecFlowArtifactsIntoAssembly(artifactFolder, specflowAssemblyName + ".dll", "Xamarin.UITest.SpecFlow.Shared.dll");

            CollectionAssert.DoesNotContain(assembliesBefore, specflowAssemblyName);

            var assembliesAfter = AppDomain.CurrentDomain.GetAssemblies().Select(e => e.GetName().Name);
            CollectionAssert.Contains(assembliesAfter, specflowAssemblyName);
        }
    }
}

