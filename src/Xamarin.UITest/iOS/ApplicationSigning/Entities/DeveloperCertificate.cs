using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
namespace Xamarin.UITest.iOS.ApplicationSigning.Entities
{
    internal class DeveloperCertificate
    {
        public readonly string CommonName;
        public readonly string SHASum;

        private static string ParseCommonNameFromSubject(string certificateSubject)
        {
            List<string> certificateSubjectPropertiesList = certificateSubject.Split(separator: ',').ToList();
            return certificateSubjectPropertiesList.Where(x => x.Contains("CN=")).First().Split(separator: '=').Last();
        }
        public DeveloperCertificate(string base64EncodedString)
        {
            byte[] certificateData = Convert.FromBase64String(s: base64EncodedString);
            X509Certificate certificate = new(data: certificateData);
            CommonName = ParseCommonNameFromSubject(certificateSubject: certificate.Subject);
            SHASum = certificate.GetCertHashString();
        }

        public CodesignIdentity GetCodesignIdentity()
        {
            return new(name: CommonName, shaSum: SHASum);
        }
    }
}

