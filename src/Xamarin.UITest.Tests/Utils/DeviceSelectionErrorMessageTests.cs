using NUnit.Framework;
using Xamarin.UITest.iOS;

namespace Xamarin.UITest.Tests.Utils
{
    [TestFixture]
    class DeviceSelectionErrorMessageTests
    {
        private string NoLineBreaks(string input) {
            return input.Replace("\n", "").Replace("\r", "");
        }

        [Test]
        public void CanParseOutputFromXcode5()
        {
            var processOutput = @"Instruments Usage Error : Unknown hardware device specified: iPhone Retina (3.5-inch) - Simulator - iOS 6.2
Known Devices:
Simon’s MacBook Pro (com.apple.instruments.devices.local)
iPhone - Simulator - iOS 6.1
iPhone - Simulator - iOS 7.1
iPhone Retina (3.5-inch) - Simulator - iOS 6.1
iPhone Retina (3.5-inch) - Simulator - iOS 7.1
iPhone Retina (4-inch) - Simulator - iOS 6.1
iPhone Retina (4-inch) - Simulator - iOS 7.1
iPhone Retina (4-inch 64-bit) - Simulator - iOS 6.1
iPhone Retina (4-inch 64-bit) - Simulator - iOS 7.1
iPad - Simulator - iOS 6.1
iPad - Simulator - iOS 7.1
iPad Retina - Simulator - iOS 6.1
iPad Retina - Simulator - iOS 7.1
iPad Retina (64-bit) - Simulator - iOS 6.1
iPad Retina (64-bit) - Simulator - iOS 7.1";
        
            var res = DeviceSelectionErrorMessage.Generate(processOutput);
            Assert.AreEqual(NoLineBreaks(res), NoLineBreaks(@"Unknown device: iPhone Retina (3.5-inch) - Simulator - iOS 6.2.
Available devices: Simon’s MacBook Pro (com.apple.instruments.devices.local)
iPhone - Simulator - iOS 6.1
iPhone - Simulator - iOS 7.1
iPhone Retina (3.5-inch) - Simulator - iOS 6.1
iPhone Retina (3.5-inch) - Simulator - iOS 7.1
iPhone Retina (4-inch) - Simulator - iOS 6.1
iPhone Retina (4-inch) - Simulator - iOS 7.1
iPhone Retina (4-inch 64-bit) - Simulator - iOS 6.1
iPhone Retina (4-inch 64-bit) - Simulator - iOS 7.1
iPad - Simulator - iOS 6.1
iPad - Simulator - iOS 7.1
iPad Retina - Simulator - iOS 6.1
iPad Retina - Simulator - iOS 7.1
iPad Retina (64-bit) - Simulator - iOS 6.1
iPad Retina (64-bit) - Simulator - iOS 7.1"));
        }

        [Test]
        public void canParseOutputFromXcode6Beta() {
            var processOutput = @"Instruments Usage Error : Unknown device specified: ""C99F2E52-35C2-5621-98B4-595780DE452E""
Known Devices:
Simon’s MacBook Pro [C99F2E52-35C2-5621-98B4-595780DE452F]
Resizable iPad (8.0 Simulator) [AB1C81AB-657A-47A3-B67C-35A292EF2001]
Resizable iPhone (8.0 Simulator) [1E3AFFCF-BF06-4678-B36A-9C07030C99FC]
iPad 2 (8.0 Simulator) [E9A28CBE-7F63-46D2-94E4-879DB232FC50]
iPad Air (8.0 Simulator) [0BC04A26-8350-4C42-A21C-561BB3D88415]
iPad Retina (8.0 Simulator) [A7B31A87-C3AC-4E7B-8E7B-E612EB3642A6]
iPhone 4s (8.0 Simulator) [D369DA75-D3B1-434F-998F-5FE5B9ABFF79]
iPhone 5 (8.0 Simulator) [37E9A991-1479-4160-86F2-CC9D9249841F]
iPhone 5s (8.0 Simulator) [68A17874-1F6D-4A05-BA2E-15F7C810D606]";

            var res = DeviceSelectionErrorMessage.Generate(processOutput);
            Assert.AreEqual(NoLineBreaks(res), NoLineBreaks(@"Unknown device: ""C99F2E52-35C2-5621-98B4-595780DE452E"".
Available devices: Simon’s MacBook Pro [C99F2E52-35C2-5621-98B4-595780DE452F]
Resizable iPad (8.0 Simulator) [AB1C81AB-657A-47A3-B67C-35A292EF2001]
Resizable iPhone (8.0 Simulator) [1E3AFFCF-BF06-4678-B36A-9C07030C99FC]
iPad 2 (8.0 Simulator) [E9A28CBE-7F63-46D2-94E4-879DB232FC50]
iPad Air (8.0 Simulator) [0BC04A26-8350-4C42-A21C-561BB3D88415]
iPad Retina (8.0 Simulator) [A7B31A87-C3AC-4E7B-8E7B-E612EB3642A6]
iPhone 4s (8.0 Simulator) [D369DA75-D3B1-434F-998F-5FE5B9ABFF79]
iPhone 5 (8.0 Simulator) [37E9A991-1479-4160-86F2-CC9D9249841F]
iPhone 5s (8.0 Simulator) [68A17874-1F6D-4A05-BA2E-15F7C810D606]"));
        }
        [Test] 
        public void canProcessOutputFromXcode6GM() {
            var processOutput = @"Known Devices:
Instruments Usage Error : Unknown device specified: ""iPhonex 5 (7.0.3 Simulator)""
Simon’s MacBook Pro [C99F2E52-35C2-5621-98B4-595780DE452F]
Resizable iPad (8.0 Simulator) [7A740247-80F3-44DA-9707-0E996814E387]
Resizable iPhone (8.0 Simulator) [7474E9BD-C182-4CB6-BF9A-7ABB5934A1CD]
iPad 2 (7.0.3 Simulator) [C434683C-25BB-4F27-8001-25D57D4276EE]
iPhone 5s (8.0 Simulator) [21F2A0BA-64D3-4D19-9182-93C3DC62FEFB]
iPhone 6 (8.0 Simulator) [BA5F797A-6541-449C-ADCC-DCA17D781731]
iPhone 6 Plus (8.0 Simulator) [EE14B293-BA7B-4FB3-84D2-F7CC30B9BF76]";
            var res = DeviceSelectionErrorMessage.Generate(processOutput);
            Assert.AreEqual(NoLineBreaks(res), NoLineBreaks(@"Unknown device: ""iPhonex 5 (7.0.3 Simulator)"".
Available devices: Simon’s MacBook Pro [C99F2E52-35C2-5621-98B4-595780DE452F]
Resizable iPad (8.0 Simulator) [7A740247-80F3-44DA-9707-0E996814E387]
Resizable iPhone (8.0 Simulator) [7474E9BD-C182-4CB6-BF9A-7ABB5934A1CD]
iPad 2 (7.0.3 Simulator) [C434683C-25BB-4F27-8001-25D57D4276EE]
iPhone 5s (8.0 Simulator) [21F2A0BA-64D3-4D19-9182-93C3DC62FEFB]
iPhone 6 (8.0 Simulator) [BA5F797A-6541-449C-ADCC-DCA17D781731]
iPhone 6 Plus (8.0 Simulator) [EE14B293-BA7B-4FB3-84D2-F7CC30B9BF76]"));
        }

    }
}
