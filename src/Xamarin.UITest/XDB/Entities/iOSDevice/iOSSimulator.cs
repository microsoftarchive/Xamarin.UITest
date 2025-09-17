using System;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Shared.iOS;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;

namespace Xamarin.UITest.XDB.Entities.iOSDevice;

internal class iOSSimulator: IiOSDevice
{
    private UDID UDID;

    public iOSSimulator(UDID deviceUDID)
    {
        UDID = deviceUDID;
    }
    public void PrepareDevice(HttpCalabashConnection httpCalabashConnection, bool shouldClearAppData)
    {
        IEnvironmentService environmentService = XdbServices.GetRequiredService<IEnvironmentService>();
        var simManager = new iOSSimulatorManagement();
        simManager.PrepareSimulator(UDID.ToString(), environmentService.AppBundlePath ?? environmentService.AppBundleId, shouldClearAppData);
    }

    public void PrepareSigningInfo() {
        return;
    }
}
