
using Xamarin.UITest.XDB.Entities;

namespace Xamarin.UITest.XDB.Services.OSX
{
    interface IiOSBundleService
    {
        IiOSAppBundle LoadBundle(string appBundlePath);
    }
}