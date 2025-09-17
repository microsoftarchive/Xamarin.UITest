using System;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.OSX;
using Xamarin.UITest.XDB.Services.OSX.IDB;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.XDB
{
    static class ServiceHelper
    {
        public static void RegisterServices(
            Action<Type, Type> registerSingletonServiceType)
        {
            registerSingletonServiceType(typeof(IDependenciesDeploymentService), typeof(DependenciesDeploymentService));
            registerSingletonServiceType(typeof(IEnvironmentService), typeof(EnvironmentService));
            registerSingletonServiceType(typeof(IiOSDeviceAgentService), typeof(iOSDeviceAgentService));
            registerSingletonServiceType(typeof(IHttpService), typeof(HttpService));
            registerSingletonServiceType(typeof(ILoggerService), typeof(LoggerService));
            registerSingletonServiceType(typeof(IProcessService), typeof(ProcessService));

            if (Shared.Processes.Platform.Instance.IsOSX)
            {
                registerSingletonServiceType(typeof(IiOSBundleService), typeof(iOSBundleService));
                registerSingletonServiceType(typeof(IIDBService), typeof(IDBService));
                registerSingletonServiceType(typeof(IPListService), typeof(PListService));
                registerSingletonServiceType(typeof(IXcodeService), typeof(XcodeService));
            }
        }
    }
}