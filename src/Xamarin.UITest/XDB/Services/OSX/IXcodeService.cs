using System;
using Xamarin.UITest.Shared.Artifacts;
using Xamarin.UITest.Shared.Resources;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB.Services.OSX
{
    interface IXcodeService
    {
        Version GetCurrentVersion();
		ProcessResult TestWithoutBuilding(
            string deviceId,
            string xctestrunPath,
            string derivedDataPath);
    }
}