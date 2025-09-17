using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Should;
using Xamarin.UITest.Shared.Android;
using Xamarin.UITest.Shared.Android.Queries;
using Xamarin.UITest.Shared.Dependencies;
using Xamarin.UITest.Shared.Execution;
using Xamarin.UITest.Shared.Processes;
using Xamarin.UITest.Shared.Resources;
using System.Reflection;

namespace Xamarin.UITest.Tests.Processes
{
    [TestFixture]
    public class AndroidSdkProcessTests
    {
        [Test]
        public void NoDevicesConnected()
        {
            var input = @"List of devices attached";

            var devices = RunQuery(new QueryAdbDevices(), input);

            devices.ShouldBeEmpty();
        }

        [Test]
        public void OneDeviceConnected()
        {
            var input = @"List of devices attached 
192.168.56.102:5555	device

";

            var devices = RunQuery(new QueryAdbDevices(), input);

            devices.Count().ShouldEqual(1);
        }

        [Test]
        public void AaptDump()
        {
            var input = @"package: name='com.lesspainful.simpleui' versionCode='1' versionName='1.0'
sdkVersion:'4'
application-label:'SimpleUI'
application-icon-120:'res/drawable-ldpi/icon.png'
application-icon-160:'res/drawable-mdpi/icon.png'
application-icon-240:'res/drawable-hdpi/icon.png'
application: label='SimpleUI' icon='res/drawable-mdpi/icon.png'
application-debuggable
launchable-activity: name='com.lesspainful.simpleui.MainActivity'  label='SimpleUI' icon=''
uses-permission:'android.permission.INTERNET'
uses-feature:'android.hardware.touchscreen'
uses-implied-feature:'android.hardware.touchscreen','assumed you require a touch screen unless explicitly made optional'
main
other-activities
supports-screens: 'small' 'normal' 'large'
supports-any-density: 'true'
locales: '--_--'
densities: '120' '160' '240'";

            var dumpResult = RunQuery(new QueryAaptDumpBadging(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("com.lesspainful.simpleui");
        }

        [Test]
        public void KeyStoreFingerprintExtraction()
        {
            var input = @"Alias name: androiddebugkey
Creation date: 05-02-2014
Entry type: PrivateKeyEntry
Certificate chain length: 1
Certificate[1]:
Owner: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Issuer: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Serial number: 39d75d24
Valid from: Wed Feb 05 12:01:52 CET 2014 until: Tue Nov 01 12:01:52 CET 2016
Certificate fingerprints:
	 MD5:  BE:EA:96:C1:74:09:1F:6E:93:34:50:34:1B:B6:D6:53
	 SHA1: 3E:2D:B5:D1:D7:D2:8D:43:84:0C:C5:28:C9:EF:DD:D2:B2:10:58:63
	 SHA256: FB:73:6E:2B:A1:64:9B:2C:71:06:AA:C6:BF:4A:F1:0F:44:CD:80:A8:92:1A:03:42:BC:A5:9D:9F:33:93:E1:5A
	 Signature algorithm name: SHA256withRSA
	 Version: 3

Extensions: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 9F 81 6C 87 FA 53 A3 87   F0 3E 11 2E 0E 9A 3A 6F  ..l..S...>....:o
0010: CE B9 0A 40                                        ...@
]
]

";

            var fingerprints = RunQuery(new QueryKeyStoreMd5Fingerprints(@"c:\debug.keystore", "test", "test"), input);

            fingerprints.Single().ShouldEqual("BE:EA:96:C1:74:09:1F:6E:93:34:50:34:1B:B6:D6:53");
        }

        [Test]
        public void KeyStoreFingerprintExtractionFrench()
        {
            var input = @"Nom d'alias : androiddebugkey
Date de crÚation : 14 janv. 2015
Type d'entrÚeá: PrivateKeyEntry
Longueur de cha¯ne du certificat : 1
Certificat[1]:
PropriÚtaireá: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
╔metteurá: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
NumÚro de sÚrieá: 54b654c5
Valide duá: Wed Jan 14 12:36:37 WAT 2015 auá: Tue Oct 10 12:36:37 WAT 2017
Empreintes du certificatá:
	 MD5á:  4E:7B:B9:BE:70:2D:C8:AD:80:E6:65:B5:8E:E9:33:B0
	 SHA1á: 98:B8:BC:D3:AE:B8:91:EE:DB:78:DE:1B:85:8E:4E:A6:DE:01:D3:6B
	 Nom de l'algorithme de signatureá: SHA1withRSA
	 Versioná: 3
";

            var fingerprints = RunQuery(new QueryKeyStoreMd5Fingerprints(@"c:\debug.keystore", "test", "test"), input);

            fingerprints.Single().ShouldEqual("4E:7B:B9:BE:70:2D:C8:AD:80:E6:65:B5:8E:E9:33:B0");
        }

        [Test]
        public void RsaFileFingerprintExtraction()
        {
            var input = @"Owner: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Issuer: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Serial number: 434f9fc3
Valid from: Mon Feb 17 14:54:39 CET 2014 until: Sun Nov 13 14:54:39 CET 2016
Certificate fingerprints:
	 MD5:  CB:DC:DE:43:28:FD:18:9F:71:B5:12:D0:7A:12:3F:AA
	 SHA1: AF:22:0B:C5:C0:40:A5:BD:E8:D3:3D:67:88:06:36:42:59:B1:58:B7
	 SHA256: 4D:F4:41:5C:BE:17:50:C7:1A:41:89:6D:DE:97:55:D4:66:A1:A3:AC:31:1F:AF:6C:05:6E:90:A5:9C:5C:EA:77
	 Signature algorithm name: SHA256withRSA
	 Version: 3

Extensions: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 77 37 B6 F2 10 76 74 4F   00 A2 AA 73 86 D8 12 C9  w7...vtO...s....
0010: B8 79 66 39                                        .yf9
]
]
";

            var fingerprints = RunQuery(new QueryRsaFileMd5Fingerprints(@"c:\debug.rsa"), input);

            fingerprints.Single().ShouldEqual("CB:DC:DE:43:28:FD:18:9F:71:B5:12:D0:7A:12:3F:AA");
        }

        [Test]
        public void RsaFileFingerprintExtractionFrench()
        {
            var input = @"PropriÚtaireá: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
╔metteurá: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
NumÚro de sÚrieá: 54b654c5
Valide duá: Wed Jan 14 12:36:37 WAT 2015 auá: Tue Oct 10 12:36:37 WAT 2017
Empreintes du certificatá:
	 MD5á:  4E:7B:B9:BE:70:2D:C8:AD:80:E6:65:B5:8E:E9:33:B0
	 SHA1á: 98:B8:BC:D3:AE:B8:91:EE:DB:78:DE:1B:85:8E:4E:A6:DE:01:D3:6B
	 Nom de l'algorithme de signatureá: SHA1withRSA
	 Versioná: 3
";

            var fingerprints = RunQuery(new QueryRsaFileMd5Fingerprints(@"c:\debug.rsa"), input);

            fingerprints.Single().ShouldEqual("4E:7B:B9:BE:70:2D:C8:AD:80:E6:65:B5:8E:E9:33:B0");
        }

        [Test]
        public void AaptPackageListExtraction()
        {
            var input = @"META-INF/MANIFEST.MF
META-INF/ANDROIDD.SF
META-INF/ANDROIDD.RSA
res/layout/foo.xml
res/layout/main.xml
AndroidManifest.xml
resources.arsc
res/drawable-hdpi/background.png
res/drawable-hdpi/icon.png
res/drawable-ldpi/icon.png
res/drawable-mdpi/icon.png
classes.dex";

            var files = RunQuery(new QueryAaptListFiles(@"c:\debug.apk"), input);

            files.Count().ShouldEqual(12);
        }

        [Test]
        public void AdbInstalledPackageSha256()
        {
            const string sha256Input = @"5d723b6e24412dc01d98e1bc9221231a  /data/app/com.lesspainful.simpleui-1.apk";

            var files = RunQuery(new QueryAdbInstalledPackageSha256("device-serial", new InstalledPackage("com.lesspainful.simpleui", "/data/app/com.lesspainful.simpleui-1.apk"), 23), sha256Input);

            files.ShouldEqual("5d723b6e24412dc01d98e1bc9221231a");
        }

        [Test]
        public void AdbInstalledPackages()
        {
            const string pathInput = @"package:/data/app/AndroidControlGallery.AndroidControlGallery-1.apk=AndroidControlGallery.AndroidControlGallery
package:/data/app/AndroidControlGallery.AndroidControlGallery.test-1.apk=AndroidControlGallery.AndroidControlGallery.tes
t
package:/data/app/Mono.Android.DebugRuntime-1.apk=Mono.Android.DebugRuntime
package:/data/app/Mono.Android.Platform.ApiLevel_18-1.apk=Mono.Android.Platform.ApiLevel_18
package:/data/app/Mono.Android.Platform.ApiLevel_8-1.apk=Mono.Android.Platform.ApiLevel_8
package:/data/app/Xamarin.UITest.TestApp.Android-1.apk=Xamarin.UITest.TestApp.Android
package:/data/app/Xamarin.UITest.TestApp.Android.test-1.apk=Xamarin.UITest.TestApp.Android.test
package:/system/framework/framework-res.apk=android
package:/data/app/ban.card.payanywhere-1.apk=ban.card.payanywhere
package:/data/app/ban.card.payanywhere.test-1.apk=ban.card.payanywhere.test
package:/data/app/ch.erni.itinerisapp-1.apk=ch.erni.itinerisapp
package:/data/app/ch.erni.itinerisapp.test-1.apk=ch.erni.itinerisapp.test
package:/system/app/vmconfig.apk=com.androVM.vmconfig
package:/system/app/BackupRestoreConfirmation.apk=com.android.backupconfirm
package:/system/app/Bluetooth.apk=com.android.bluetooth
package:/system/app/Browser.apk=com.android.browser
package:/system/app/Calculator.apk=com.android.calculator2
package:/system/app/Calendar.apk=com.android.calendar
package:/system/app/LegacyCamera.apk=com.android.camera
package:/system/app/CertInstaller.apk=com.android.certinstaller
package:/system/app/Contacts.apk=com.android.contacts
package:/system/app/DefaultContainerService.apk=com.android.defcontainer
package:/system/app/DeskClock.apk=com.android.deskclock
package:/system/app/BasicDreams.apk=com.android.dreams.basic
package:/system/app/PhotoTable.apk=com.android.dreams.phototable
package:/system/app/Email.apk=com.android.email
package:/system/app/Exchange2.apk=com.android.exchange
package:/system/app/Galaxy4.apk=com.android.galaxy4
package:/system/app/Gallery.apk=com.android.gallery
package:/data/app/GestureBuilder.apk=com.android.gesture.builder
package:/system/app/HTMLViewer.apk=com.android.htmlviewer
package:/system/app/InputDevices.apk=com.android.inputdevices
package:/system/app/LatinIME.apk=com.android.inputmethod.latin
package:/system/app/PinyinIME.apk=com.android.inputmethod.pinyin
package:/system/app/KeyChain.apk=com.android.keychain
package:/system/app/Launcher2.apk=com.android.launcher
package:/system/app/FusedLocation.apk=com.android.location.fused
package:/system/app/MagicSmokeWallpapers.apk=com.android.magicsmoke
package:/system/app/Mms.apk=com.android.mms
package:/system/app/Music.apk=com.android.music
package:/system/app/MusicFX.apk=com.android.musicfx
package:/system/app/VisualizationWallpapers.apk=com.android.musicvis
package:/system/app/NoiseField.apk=com.android.noisefield
package:/system/app/PackageInstaller.apk=com.android.packageinstaller
package:/system/app/PhaseBeam.apk=com.android.phasebeam
package:/system/app/Phone.apk=com.android.phone
package:/system/app/ApplicationsProvider.apk=com.android.providers.applications
package:/system/app/CalendarProvider.apk=com.android.providers.calendar
package:/system/app/ContactsProvider.apk=com.android.providers.contacts
package:/system/app/DownloadProvider.apk=com.android.providers.downloads
package:/system/app/DownloadProviderUi.apk=com.android.providers.downloads.ui
package:/system/app/DrmProvider.apk=com.android.providers.drm
package:/system/app/MediaProvider.apk=com.android.providers.media
package:/system/app/SettingsProvider.apk=com.android.providers.settings
package:/system/app/TelephonyProvider.apk=com.android.providers.telephony
package:/system/app/UserDictionaryProvider.apk=com.android.providers.userdictionary
package:/system/app/Provision.apk=com.android.provision
package:/system/app/QuickSearchBox.apk=com.android.quicksearchbox
package:/system/app/Settings.apk=com.android.settings
package:/system/app/SharedStorageBackup.apk=com.android.sharedstoragebackup
package:/system/app/WAPPushManager.apk=com.android.smspush
package:/system/app/SoundRecorder.apk=com.android.soundrecorder
package:/system/app/SystemUI.apk=com.android.systemui
package:/system/app/VideoEditor.apk=com.android.videoeditor
package:/system/app/VoiceDialer.apk=com.android.voicedialer
package:/system/app/VpnDialogs.apk=com.android.vpndialogs
package:/system/app/LiveWallpapers.apk=com.android.wallpaper
package:/system/app/HoloSpiralWallpaper.apk=com.android.wallpaper.holospiral
package:/system/app/LiveWallpapersPicker.apk=com.android.wallpaper.livepicker
package:/data/app/com.angusanywhere.esi-1.apk=com.angusanywhere.esi
package:/data/app/com.angusanywhere.esi.test-1.apk=com.angusanywhere.esi.test
package:/data/app/com.buschgardens.mobile-1.apk=com.buschgardens.mobile
package:/data/app/com.buschgardens.mobile.test-1.apk=com.buschgardens.mobile.test
package:/data/app/com.bwinlabs.betdroid-1.apk=com.bwinlabs.betdroid
package:/data/app/com.bwinlabs.betdroid.test-1.apk=com.bwinlabs.betdroid.test
package:/data/app/com.compuware.apm.mobile.android-1.apk=com.compuware.apm.mobile.android
package:/data/app/com.compuware.apm.mobile.android.test-1.apk=com.compuware.apm.mobile.android.test
package:/system/app/CMFileManager.apk=com.cyanogenmod.filemanager
package:/data/app/com.dropbox.android-1.apk=com.dropbox.android
package:/data/app/com.dropbox.android.test-1.apk=com.dropbox.android.test
package:/data/app/com.evernote-1.apk=com.evernote
package:/data/app/com.evernote.test-1.apk=com.evernote.test
package:/data/app/ApiDemos.apk=com.example.android.apis
package:/system/app/CubeLiveWallpapers.apk=com.example.android.livecubes
package:/system/app/ClipboardProxy.apk=com.genymotion.clipboardproxy
package:/data/app/com.google.android.googlequicksearchbox-1.apk=com.google.android.googlequicksearchbox
package:/data/app/com.google.android.googlequicksearchbox.test-1.apk=com.google.android.googlequicksearchbox.test
package:/data/app/com.howaboutwe.singles-1.apk=com.howaboutwe.singles
package:/data/app/com.howaboutwe.singles.test-1.apk=com.howaboutwe.singles.test
package:/data/app/com.insightly.droid-1.apk=com.insightly.droid
package:/data/app/com.insightly.droid.test-1.apk=com.insightly.droid.test
package:/data/app/com.lesspainful.simpleui-1.apk=com.lesspainful.simpleui
package:/data/app/com.lesspainful.simpleui.test-1.apk=com.lesspainful.simpleui.test
package:/data/app/com.mint-1.apk=com.mint
package:/data/app/com.mint.test-1.apk=com.mint.test
package:/data/app/com.niposoftware.capi.client.beta-1.apk=com.niposoftware.capi.client.beta
package:/data/app/com.niposoftware.capi.client.beta.test-1.apk=com.niposoftware.capi.client.beta.test
package:/data/app/com.rdio.android.ui-1.apk=com.rdio.android.ui
package:/data/app/com.rdio.android.ui.test-1.apk=com.rdio.android.ui.test
package:/data/app/com.schneider_electric.wiserems-1.apk=com.schneider_electric.wiserems
package:/data/app/com.schneider_electric.wiserems.test-1.apk=com.schneider_electric.wiserems.test
package:/data/app/com.seaworld.mobile-1.apk=com.seaworld.mobile
package:/data/app/com.seaworld.mobile.test-1.apk=com.seaworld.mobile.test
package:/system/app/PicoTts.apk=com.svox.pico
package:/system/app/Superuser.apk=com.thirdparty.superuser
package:/data/app/com.xamarin.XamStore-1.apk=com.xamarin.XamStore
package:/data/app/com.xamarin.XamStore.test-1.apk=com.xamarin.XamStore.test
package:/data/app/com.xamarin.samples.taskydroid-1.apk=com.xamarin.samples.taskydroid
package:/data/app/com.xamarin.samples.taskydroid.test-1.apk=com.xamarin.samples.taskydroid.test
package:/system/app/OpenWnn.apk=jp.co.omronsoft.openwnn
package:/data/app/me.lyft.android-1.apk=me.lyft.android
package:/data/app/me.lyft.android.test-1.apk=me.lyft.android.test
package:/data/app/rc.appradio.android-1.apk=rc.appradio.android
package:/data/app/rc.appradio.android.test-1.apk=rc.appradio.android.test";

            var installedPackages = RunQuery(new QueryAdbInstalledPackages("device-serial"), pathInput);

            installedPackages.Any(x => x.ApkPath == "/system/app/Superuser.apk" && x.Package == "com.thirdparty.superuser").ShouldBeTrue();
        }

        [Test]
        public void XmltreeDumpTasky()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "tasky-xmltree.txt");

            var dumpResult = RunQuery(new QueryAaptDumpXmltreeManifest(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("com.xamarin.samples.taskydroid");
        }

        [Test]
        public void XmltreeDumpFlipboard()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "flipboard-xmltree.txt");

            var dumpResult = RunQuery(new QueryAaptDumpXmltreeManifest(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("flipboard.app");
        }


        [Test]
        public void Bug1DumpFlipboard()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "bug1-xmltree.txt");

            var dumpResult = RunQuery(new QueryAaptDumpXmltreeManifest(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("com.xamarin.nineoldandroids.samples");
        }

        [Test]
        public void FindMultipleLaunchableActivitiesAaptBadging()
        {
            var input = @"package: name='Mono.Samples.HelloTests' versionCode='1' versionName='1.0'
sdkVersion:'4'
application-label:'SimpleUI'
application-icon-120:'res/drawable-ldpi/icon.png'
application-icon-160:'res/drawable-mdpi/icon.png'
application-icon-240:'res/drawable-hdpi/icon.png'
application: label='SimpleUI' icon='res/drawable-mdpi/icon.png'
application-debuggable
launchable-activity: name='mono.samples.HelloApp'  label='SimpleUI' icon=''
launchable-activity: name='mono.samples.hello.LibraryActivity' label='SimpleUI' icon=''
uses-permission:'android.permission.INTERNET'
uses-feature:'android.hardware.touchscreen'
uses-implied-feature:'android.hardware.touchscreen','assumed you require a touch screen unless explicitly made optional'
main
other-activities
supports-screens: 'small' 'normal' 'large'
supports-any-density: 'true'
locales: '--_--'
densities: '120' '160' '240'";

            var dumpResult = RunQuery(new QueryAaptDumpBadging(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("Mono.Samples.HelloTests");
        }

        [Test]
        public void FindMultipleLaunchableActivitiesAaptDumpXmlManifest()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "multiple-launchable-activities.txt");

            var dumpResult = RunQuery(new QueryAaptDumpXmltreeManifest(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.PackageName.ShouldEqual("Mono.Samples.HelloTests");
        }

        [Test]
        public void GetPermissionsAaptDumpXmlManifest()
        {
            var loader = new EmbeddedResourceLoader();

            var input = loader.GetEmbeddedResourceString(Assembly.GetExecutingAssembly(), "multiple-launchable-activities.txt");

            var dumpResult = RunQuery(new QueryAaptDumpXmltreeManifest(new ApkFile(@"c:\test.apk", null)), input);

            var permissions = dumpResult.Permissions;

            dumpResult.Permissions.Single().ShouldEqual("android.permission.INTERNET");
        }

        [Test]
        public void GetPermissionsAaptDumpBadging()
        {
            var input = @"package: name='com.lesspainful.simpleui' versionCode='1' versionName='1.0'
sdkVersion:'4'
application-label:'SimpleUI'
application-icon-120:'res/drawable-ldpi/icon.png'
application-icon-160:'res/drawable-mdpi/icon.png'
application-icon-240:'res/drawable-hdpi/icon.png'
application: label='SimpleUI' icon='res/drawable-mdpi/icon.png'
application-debuggable
launchable-activity: name='com.lesspainful.simpleui.MainActivity'  label='SimpleUI' icon=''
uses-permission:'android.permission.INTERNET'
uses-feature:'android.hardware.touchscreen'
uses-implied-feature:'android.hardware.touchscreen','assumed you require a touch screen unless explicitly made optional'
main
other-activities
supports-screens: 'small' 'normal' 'large'
supports-any-density: 'true'
locales: '--_--'
densities: '120' '160' '240'";

            var dumpResult = RunQuery(new QueryAaptDumpBadging(new ApkFile(@"c:\test.apk", null)), input);

            dumpResult.Permissions.Single().ShouldEqual("android.permission.INTERNET");
        }

        [Test]
        public void KeyStoreSha256FingerprintExtraction()
        {
            var input = @"Alias name: androiddebugkey
Creation date: Apr 17, 2015
Entry type: PrivateKeyEntry
Certificate chain length: 1
Certificate[1]:
Owner: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Issuer: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Serial number: 39f10720
Valid from: Fri Apr 17 11:38:41 MSK 2015 until: Sat Mar 14 11:38:41 MSK 4753
Certificate fingerprints:
     SHA1: 71:DB:9E:29:31:04:DE:73:E4:9C:F0:B3:79:19:FF:32:C8:EF:F8:93
     SHA256: 7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9
Signature algorithm name: SHA256withRSA
Subject Public Key Algorithm: 2048-bit RSA key
Version: 3

Extensions: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 90 7D 76 A7 48 FD BB BF   CF 9E 92 A0 CB C5 2B 35  ..v.H.........+5
0010: 12 FE 57 B2                                        ..W.
]
]

";

            var fingerprints = RunQuery(new QueryKeyStoreFingerprints(@"c:\debug.keystore", "test", "test"), input);

            fingerprints.ShouldContain("7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9");
        }

        [Test]
        public void KeyStoreSha256FingerprintExtractionFrench()
        {
            var input = @"Nom d'alias : androiddebugkey
Date de création : 17 avr. 2015
Type d'entrée : PrivateKeyEntry
Longueur de chaîne du certificat : 1
Certificat[1]:
Propriétaire : CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Emetteur : CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Numéro de série : 39f10720
Valide du Fri Apr 17 11:38:41 MSK 2015 au Sat Mar 14 11:38:41 MSK 4753
Empreintes du certificat :
     SHA 1: 71:DB:9E:29:31:04:DE:73:E4:9C:F0:B3:79:19:FF:32:C8:EF:F8:93
     SHA 256: 7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9
Nom de l'algorithme de signature : SHA256withRSA
Algorithme de clé publique du sujet : Clé RSA 2048 bits
Version : 3

Extensions : 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 90 7D 76 A7 48 FD BB BF   CF 9E 92 A0 CB C5 2B 35  ..v.H.........+5
0010: 12 FE 57 B2                                        ..W.
]
]

";

            var fingerprints = RunQuery(new QueryKeyStoreFingerprints(@"c:\debug.keystore", "test", "test"), input);

            fingerprints.ShouldContain("7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9");
        }

        [Test]
        public void KeyStoreSha256FingerprintExtractionChinese()
        {
            var input = @"Alias name: androiddebugkey
Creation date: 2015年4月17日
Entry type: PrivateKeyEntry
Certificate chain length: 1
Certificate[1]:
Owner: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Issuer: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Serial number: 39f10720
Valid from: Fri Apr 17 11:38:41 MSK 2015 until: Sat Mar 14 11:38:41 MSK 4753
Certificate fingerprints:
     SHA1: 71:DB:9E:29:31:04:DE:73:E4:9C:F0:B3:79:19:FF:32:C8:EF:F8:93
     SHA256: 7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9
Signature algorithm name: SHA256withRSA
Subject Public Key Algorithm: 2048-bit RSA key
Version: 3

Extensions: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 90 7D 76 A7 48 FD BB BF   CF 9E 92 A0 CB C5 2B 35  ..v.H.........+5
0010: 12 FE 57 B2                                        ..W.
]
]

";

            var fingerprints = RunQuery(new QueryKeyStoreFingerprints(@"c:\debug.keystore", "test", "test"), input);

            fingerprints.ShouldContain("7F:30:BC:13:C6:32:CF:62:C3:43:85:7D:D5:83:83:61:26:03:BC:FB:C8:4B:7E:A9:ED:D3:F9:47:F4:9C:EE:C9");
        }

        [Test]
        public void RsaFileSha256FingerprintExtraction()
        {
            var input = @"Owner: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Issuer: CN=dev.xamarin.com, OU=DEV, O=XAM, L=SF, ST=CA, C=US
Serial number: 434f9fc3
Valid from: Mon Feb 17 14:54:39 CET 2014 until: Sun Nov 13 14:54:39 CET 2016
Certificate fingerprints:
     MD5:  CB:DC:DE:43:28:FD:18:9F:71:B5:12:D0:7A:12:3F:AA
     SHA1: AF:22:0B:C5:C0:40:A5:BD:E8:D3:3D:67:88:06:36:42:59:B1:58:B7
     SHA256: 4D:F4:41:5C:BE:17:50:C7:1A:41:89:6D:DE:97:55:D4:66:A1:A3:AC:31:1F:AF:6C:05:6E:90:A5:9C:5C:EA:77
     Signature algorithm name: SHA256withRSA
     Version: 3

Extensions: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 77 37 B6 F2 10 76 74 4F   00 A2 AA 73 86 D8 12 C9  w7...vtO...s....
0010: B8 79 66 39                                        .yf9
]
]
";

            var fingerprints = RunQuery(new QueryRsaFileFingerprints(@"c:\debug.rsa"), input);

            fingerprints.ShouldContain("4D:F4:41:5C:BE:17:50:C7:1A:41:89:6D:DE:97:55:D4:66:A1:A3:AC:31:1F:AF:6C:05:6E:90:A5:9C:5C:EA:77");
        }


        [Test]
        public void RsaFileSha256FingerprintExtractionFrench()
        {
            var input = @"Propriétaire : CN=1 2, OU=1 2, O=1 2, L=1 2, ST=1 2, C=FR
Emetteur : CN=1 2, OU=1 2, O=1 2, L=1 2, ST=1 2, C=FR
Numéro de série : 6139227c
Valide du Wed Jan 31 10:21:00 MSK 2018 au Tue May 01 10:21:00 MSK 2018
Empreintes du certificat :
     SHA 1: 3A:78:D5:B6:17:B9:32:6B:60:F4:AE:E0:14:BD:2E:AA:E1:08:6F:26
     SHA 256: 2C:51:D1:94:2B:DA:EE:E6:2D:0F:D9:72:E1:BA:6E:32:79:87:2D:52:B8:C3:7F:90:44:44:2E:45:93:7E:52:60
Nom de l'algorithme de signature : SHA256withDSA
Algorithme de clé publique du sujet : Clé DSA 2048 bits
Version : 3

Extensions : 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 0F F8 C6 82 9A A5 3B 9B   18 BF D7 5B E6 D6 12 3D  ......;....[...=
0010: E5 FD AC 36                                        ...6
]
]";

            var fingerprints = RunQuery(new QueryRsaFileFingerprints(@"c:\debug.rsa"), input);

            fingerprints.ShouldContain("2C:51:D1:94:2B:DA:EE:E6:2D:0F:D9:72:E1:BA:6E:32:79:87:2D:52:B8:C3:7F:90:44:44:2E:45:93:7E:52:60");
        }

        [Test]
        public void RsaFileSha256FingerprintExtractionChineseSimplified()
        {
            var input = @"所有者: CN=1 2, OU=1 2, O=1 2, L=1 2, ST=1 2, C=FR
发布者: CN=1 2, OU=1 2, O=1 2, L=1 2, ST=1 2, C=FR
序列号: 6139227c
生效时间: Wed Jan 31 10:21:00 MSK 2018, 失效时间: Tue May 01 10:21:00 MSK 2018
证书指纹:
     SHA1: 3A:78:D5:B6:17:B9:32:6B:60:F4:AE:E0:14:BD:2E:AA:E1:08:6F:26
     SHA256: 2C:51:D1:94:2B:DA:EE:E6:2D:0F:D9:72:E1:BA:6E:32:79:87:2D:52:B8:C3:7F:90:44:44:2E:45:93:7E:52:60
签名算法名称: SHA256withDSA
主体公共密钥算法: 2048 位 DSA 密钥
版本: 3

扩展: 

#1: ObjectId: 2.5.29.14 Criticality=false
SubjectKeyIdentifier [
KeyIdentifier [
0000: 0F F8 C6 82 9A A5 3B 9B   18 BF D7 5B E6 D6 12 3D  ......;....[...=
0010: E5 FD AC 36                                        ...6
]
]
";
            var fingerprints = RunQuery(new QueryRsaFileFingerprints(@"c:\debug.rsa"), input);

            fingerprints.ShouldContain("2C:51:D1:94:2B:DA:EE:E6:2D:0F:D9:72:E1:BA:6E:32:79:87:2D:52:B8:C3:7F:90:44:44:2E:45:93:7E:52:60");
        }

        // TODO: acroos
        // Add test for multiple launchable activities

        static T RunQuery<T, TD1, TD2, TD3>( IQuery<T, TD1, TD2, TD3> query, params string[] inputs )
            where TD3 : class
            where TD2 : class
            where TD1 : class
        {
            var processRunner = Substitute.For<IProcessRunner>();

            var processResults = inputs
                .Select( x => new ProcessResult( new[] { new ProcessOutput( x), }, 0, 0L, true ) )
                .ToArray();

            if ( processResults.Any() )
            {
                processRunner
                    .Run( Arg.Any<string>(), Arg.Any<string>() )
                    .ReturnsForAnyArgs( processResults.First(), processResults.Skip( 1 ).ToArray() );
            }

            var androidSdkTools = Substitute.For<IAndroidSdkTools>();
            var jdkTools = Substitute.For<IJdkTools>();

            var executor = ExecutorHelper.GetDefault( jdkTools, processRunner, androidSdkTools );

            return executor.Execute( query );
        }

        static T RunQuery<T, TD1, TD2>(IQuery<T, TD1, TD2> query, params string[] inputs)
            where TD2 : class
            where TD1 : class
        {
            var processRunner = Substitute.For<IProcessRunner>();

            var processResults = inputs
                .Select(x => new ProcessResult(new[] { new ProcessOutput(x), }, 0, 0L, true))
                .ToArray();

            if (processResults.Any())
            {
                processRunner
                    .Run(Arg.Any<string>(), Arg.Any<string>())
                    .ReturnsForAnyArgs(processResults.First(), processResults.Skip(1).ToArray());
            }

            var androidSdkTools = Substitute.For<IAndroidSdkTools>();
            var jdkTools = Substitute.For<IJdkTools>();

            var executor = ExecutorHelper.GetDefault(jdkTools, processRunner, androidSdkTools);

            return executor.Execute(query);
        }

        static T RunQuery<T, TD1>(IQuery<T, TD1> query, params string[] inputs)
            where TD1 : class
        {
            var processRunner = Substitute.For<IProcessRunner>();

            var processResults = inputs
                .Select(x => new ProcessResult(new[] { new ProcessOutput(x), }, 0, 0L, true))
                .ToArray();

            if (processResults.Any())
            {
                processRunner
                    .Run(Arg.Any<string>(), Arg.Any<string>())
                    .ReturnsForAnyArgs(processResults.First(), processResults.Skip(1).ToArray());
            }

            var androidSdkTools = Substitute.For<IAndroidSdkTools>();
            var jdkTools = Substitute.For<IJdkTools>();

            var executor = ExecutorHelper.GetDefault(jdkTools, processRunner, androidSdkTools);

            return executor.Execute(query);
        }
    }
}