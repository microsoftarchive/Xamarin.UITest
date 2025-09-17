using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.UITest.iOS;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.iOS.ApplicationSigning.Managers;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;

namespace Xamarin.UITest.XDB.Entities.iOSDevice;

internal class iOSPhysicalDevice: IiOSDevice
{
    private UDID UDID;
    private IEnvironmentService EnvironmentService = XdbServices.GetRequiredService<IEnvironmentService>();
    private ILoggerService LoggerService = XdbServices.GetRequiredService<ILoggerService>();
    private IIDBService IDBService = XdbServices.GetRequiredService<IIDBService>();

    public iOSPhysicalDevice(UDID deviceUDID)
    {
        UDID = deviceUDID;
    }

    private void PrepareProvisioningProfile()
    {
        if (EnvironmentService.ProvisioningProfile == null)
        {
            ProvisioningProfile provisioningProfileFromUserApp = ApplicationSigningManager.ExtractProfile(appBundlePath: EnvironmentService.AppBundlePath);
            EnvironmentService.ProvisioningProfile = provisioningProfileFromUserApp;
        }
    }

    private void PrepareCodesignIdentity()
    {
        if (EnvironmentService.CodesignIdentity == null)
        {
            if (EnvironmentService.ProvisioningProfile == null) {
                throw new Exception("Unable to extract codesign identity from provisioning profile: No profle set");
            }
            EnvironmentService.CodesignIdentity = EnvironmentService.ProvisioningProfile.ExtractCodesignIdentity();
        }
    }

    public void PrepareDevice(HttpCalabashConnection httpCalabashConnection, bool shouldClearAppData)
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(() =>
#pragma warning restore CS4014
        {
            try { httpCalabashConnection.Exit(); }
            catch { }
        });

        if (shouldClearAppData)
        {
            Thread.Sleep(500);
            LoggerService.LogDebug("Clearing app data.");
            IDBService.ClearXCAppData(UDID, EnvironmentService.AppBundleId);
        }
    }
    public void PrepareSigningInfo()
    {
        PrepareProvisioningProfile();
        PrepareCodesignIdentity();
    }
}
