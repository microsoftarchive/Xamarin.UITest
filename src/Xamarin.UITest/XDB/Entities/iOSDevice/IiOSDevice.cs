using Xamarin.UITest.iOS;

namespace Xamarin.UITest.XDB.Entities.iOSDevice;

internal interface IiOSDevice
{
    public void PrepareDevice(HttpCalabashConnection httpCalabashConnection, bool shouldClearAppData);
    public void PrepareSigningInfo();
}
