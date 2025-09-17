using System;
using System.IO;

namespace Xamarin.UITest.Android
{
    /// <summary>
    /// Represents runtime information about the currently running device.
    /// </summary>
    public class AndroidConfig
    {
        readonly Uri _deviceUri;
        readonly FileInfo _apkFile;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="deviceUri">The uri of the device.</param>
        /// <param name="apkFile">The running apk file.</param>
        public AndroidConfig(Uri deviceUri, FileInfo apkFile)
        {
            _deviceUri = deviceUri;
            _apkFile = apkFile;
        }

        /// <summary>
        /// The uri of the device.
        /// </summary>
        public Uri DeviceUri
        {
            get { return _deviceUri; }
        }

        /// <summary>
        /// The currently running apk file.
        /// </summary>
        [Obsolete("Removed")]
        public FileInfo ApkFile
        {
            get { return _apkFile; }
        }
    }
}