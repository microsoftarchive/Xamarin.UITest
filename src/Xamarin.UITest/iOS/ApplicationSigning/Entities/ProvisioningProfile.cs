using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xamarin.UITest.MacOS.Utilities;
using Xamarin.UITest.MacOS.Utilities.Xcrun;
using Xamarin.UITest.XDB;
using Xamarin.UITest.XDB.Services;
using Xamarin.UITest.XDB.Services.Processes;

namespace Xamarin.UITest.iOS.ApplicationSigning.Entities
{
    internal class ProvisioningProfile
    {
        IProcessService ProcessService = XdbServices.GetRequiredService<IProcessService>();
        ILoggerService LoggerService = XdbServices.GetRequiredService<ILoggerService>();

        public FileInfo ProvisioningProfileFile;
        public FileInfo DecodedProvisioningProfilePlistFile;
        public FileInfo ExtractedEntitlementsPlistFile;

        public List<DeveloperCertificate> DeveloperCertificates;

        public ProvisioningProfile(FileInfo provisioningProfileFile)
        {
            ProvisioningProfileFile = provisioningProfileFile;

            DecodedProvisioningProfilePlistFile = DecodeProvisioningProfileToPlistFile()
                ?? throw new Exception(message: "Unable to decode provisioning profile with xcrun security tool");

            ExtractedEntitlementsPlistFile = ExtractEntitlementsToPlistFile();
            DeveloperCertificates = ExtractDeveloperCertificate();
        }

        private FileInfo DecodeProvisioningProfileToPlistFile()
        {
            return Security.DecodeProvisioningProfileWithCMS(
                processService: ProcessService,
                loggerService: LoggerService,
                inputFile: ProvisioningProfileFile);
        }

        private FileInfo ExtractEntitlementsToPlistFile()
        {
            string entitlements = PlistBuddy.PrintEntitlements(provisioningProfileDecodedPlist: DecodedProvisioningProfilePlistFile);

            if (string.IsNullOrWhiteSpace(entitlements))
            {
                throw new Exception(message: $"Unable to extract entitlements from decoded provisioning profile plist file {DecodedProvisioningProfilePlistFile.FullName}.");
            }

            string entitlementsPlistPath = Path.Combine(path1: DecodedProvisioningProfilePlistFile.DirectoryName!, path2: "entitlements.plist");
            LoggerService.LogInfo("Entitlements will be extracted to:");
            LoggerService.LogInfo(entitlementsPlistPath);

            using StreamWriter streamWriter = File.CreateText(path: entitlementsPlistPath);
            streamWriter.Write(value: entitlements);
            streamWriter.Close();

            return new FileInfo(fileName: entitlementsPlistPath);
        }

        private List<DeveloperCertificate> ExtractDeveloperCertificate()
        {
            string developerCertificatesPlist = PlistBuddy.PrintDeveloperCertificates(provisioningProfileDecodedPlist: DecodedProvisioningProfilePlistFile);

            if (string.IsNullOrWhiteSpace(value: developerCertificatesPlist))
            {
                throw new Exception(message: $"Unable to extract developer certificates from decoded provisioning profile plist file {DecodedProvisioningProfilePlistFile.FullName}");
            }

            XDocument xDocument;
            try
            {
                xDocument = XDocument.Parse(developerCertificatesPlist);
            }
            catch
            {
                throw new ArgumentException("Invalid XML", nameof(developerCertificatesPlist));
            }

            List<XElement> developerCertificatesEncodedStringsList = xDocument.Descendants("data").ToList();

            List<DeveloperCertificate> developerCertificatesList = new();
            foreach (XElement developerCertificateEncodedString in developerCertificatesEncodedStringsList)
            {
                developerCertificatesList.Add(item: new DeveloperCertificate(base64EncodedString: developerCertificateEncodedString.Value));
            }

            return developerCertificatesList;
        }

        public CodesignIdentity ExtractCodesignIdentity()
        {
            return DeveloperCertificates.First().GetCodesignIdentity();
        }
    }
}