using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.UITest.iOS.ApplicationSigning.Entities;
using Xamarin.UITest.iOS.Entities;
using Xamarin.UITest.MacOS.Utilities.Xcrun;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.iOS.ApplicationSigning.Managers
{
    internal static class ApplicationSigningManager
    {
        private static void SignFrameworks(IProcessService processService, ILoggerService loggerService, ApplicationBundle applicationBundle, CodesignIdentity codesignIdentity, FileInfo entitlementsPlistFile)
        {
            List<DirectoryInfo> frameworksBundles = applicationBundle.GetEmbeddedFrameworks();
            if (frameworksBundles.Any())
            {
                loggerService.LogInfo(message: $"Resigning Frameworks in application bundle{Environment.NewLine}");
                foreach(DirectoryInfo frameworkBundle in frameworksBundles)
                {
                    SignItem(
                        processService: processService,
                        loggerService: loggerService,
                        item: frameworkBundle,
                        codesignIdentity: codesignIdentity,
                        entitlementsPlistFile: entitlementsPlistFile);
                }
            }
        }

        private static void SignXCTestFilesInPluginsDirectory(IProcessService processService, ILoggerService loggerService, ApplicationBundle applicationBundle, ProvisioningProfile provisioningProfile, CodesignIdentity codesignIdentity)
        {
            List<DirectoryInfo> xctestBundles = applicationBundle.GetXCTestBundles();
            if (xctestBundles.Any())
            {
                loggerService.LogInfo(message: $"Resigning .xctest files in application bundle's Plugins directory{Environment.NewLine}");
                foreach (DirectoryInfo xctestBundle in xctestBundles)
                {
                    ApplicationBundle xctestApplicationBundle = new(appBundlePath: xctestBundle.FullName);
                    loggerService.LogInfo(message: $"Resigning: {xctestBundle.FullName}");
                    SignBundle(
                        processService: processService,
                        loggerService: loggerService,
                        bundle: xctestApplicationBundle,
                        provisioningProfile: provisioningProfile,
                        codesignIdentity: codesignIdentity);
                }
            }
        }

        private static void SignDylibFiles(IProcessService processService, ILoggerService loggerService, ApplicationBundle applicationBundle, CodesignIdentity codesignIdentity, FileInfo entitlementsPlistFile)
        {
            List<FileInfo> dylibFiles = applicationBundle.GetDylibsFromFrameworksDirectory();
            if (dylibFiles.Any())
            {
                loggerService.LogInfo(message: $"Resigning .dylibs in application bundle{Environment.NewLine}");
                foreach (FileInfo dylibFile in dylibFiles)
                {
                    SignItem(
                        processService: processService,
                        loggerService: loggerService,
                        item: dylibFile,
                        codesignIdentity: codesignIdentity,
                        entitlementsPlistFile: entitlementsPlistFile);
                }
            }
        }

        private static void SignItem(
            IProcessService processService,
            ILoggerService loggerService,
            FileSystemInfo item,
            CodesignIdentity codesignIdentity,
            FileInfo entitlementsPlistFile)
        {
            loggerService.LogInfo($"Removing old signature from: {item.FullName}");
            Codesign.RemoveSignature(
                processService: processService,
                loggerService: loggerService,
                file: item);

            loggerService.LogInfo(message: $"Resigning: {item.FullName}");
            Codesign.Sign(
                processService: processService,
                loggerService: loggerService,
                fileToSign: item,
                identity: codesignIdentity,
                entitlementsPlistFile: entitlementsPlistFile);
        }

        public static ProvisioningProfile ExtractProfile(string appBundlePath)
        {
            IEnvironmentService environmentService = XdbServices.GetRequiredService<IEnvironmentService>();
            string deviceAgentResigningTempFolderPath = environmentService.GetTempFolderForDeviceAgentResigning();

            // Then try to extract it from AUT Bundle.
            if (!string.IsNullOrWhiteSpace(appBundlePath))
            {
                ApplicationBundle userApplicationBundle = new(appBundlePath: appBundlePath);
                try
                {
                    FileInfo provisioningProfileFile = userApplicationBundle.ExtractEmbeddedProvisioningProfile(extractionPath: deviceAgentResigningTempFolderPath);
                    return new(provisioningProfileFile: provisioningProfileFile);
                }
                catch (Exception)
                {
                    throw new Exception(message: $"Could not extract embedded.mobileprovision from {userApplicationBundle.AppBundle}. " +
                        $"To resolve this issue try to provide provisioning profile explicitly with iOSAppConfigurator.ProvisioningProfile method.");
                }
            }
            else
            {
                throw new Exception("Could not get provisioning profile for DeviceAgent resigning. " +
                    "Either specify Application Bundle path or specify the path to provisioning profile file.");
            }
        }

        public static void SignBundle(
            IProcessService processService,
            ILoggerService loggerService,
            ApplicationBundle bundle,
            ProvisioningProfile provisioningProfile,
            CodesignIdentity codesignIdentity)
        {
            // Replace existing embedded.mobileprovision with new provisioning profile.
            bundle.ReplaceEmbeddedProvisioningProfile(newProvisioningProfile: provisioningProfile);

            // Resign frameworks.
            SignFrameworks(
                processService: processService,
                loggerService: loggerService,
                applicationBundle: bundle,
                codesignIdentity: codesignIdentity,
                entitlementsPlistFile: provisioningProfile.ExtractedEntitlementsPlistFile);

            // Resign .xctest files in Plugins directory.
            SignXCTestFilesInPluginsDirectory(
                processService: processService,
                loggerService: loggerService,
                applicationBundle: bundle,
                provisioningProfile: provisioningProfile,
                codesignIdentity: codesignIdentity);

            // Resign .dylibs.
            SignDylibFiles(
                processService: processService,
                loggerService: loggerService,
                applicationBundle: bundle,
                codesignIdentity: codesignIdentity,
                entitlementsPlistFile: provisioningProfile.ExtractedEntitlementsPlistFile);

            // Resign main application bundle executable.
            SignItem(
                processService: processService,
                loggerService: loggerService,
                item: bundle.AppBundle,
                codesignIdentity: codesignIdentity,
                entitlementsPlistFile: provisioningProfile.ExtractedEntitlementsPlistFile);
        }
    }
}