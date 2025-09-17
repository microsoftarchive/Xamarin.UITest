using System;
using System.IO;
using Xamarin.UITest.Shared.Logging;

namespace Xamarin.UITest.Shared.Dependencies
{
    public class PotentialLocation
    {
        readonly string _path;
        readonly string _source;

        public PotentialLocation(string path, string source)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(path);
                }
                catch (Exception e)
                {
                    Log.Info($"Error creating directory info from path: \"{path}\"", e);
                    path = string.Empty;
                }
            }
            else
            {
                path = string.Empty;
            }

            _source = source;
            _path = path;
        }

        public string Path
        {
            get { return _path; }
        }

        public string Source
        {
            get { return _source; }
        }

        public override string ToString()
        {
            return string.Format("{0} [ Source: {1} ]", Path, Source);
        }
    }
}