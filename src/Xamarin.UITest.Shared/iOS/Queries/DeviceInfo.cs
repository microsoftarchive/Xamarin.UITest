using System.Text.RegularExpressions;

namespace Xamarin.UITest.Shared.iOS.Queries
{
    public class DeviceInfo
	{
        static readonly Regex _versionRegex = new Regex(@"^(\d+).*"); 
        readonly string _uuid;
        readonly string _buildVersion;

        public DeviceInfo(string uuid, string buildVersion)
        {
            _buildVersion = buildVersion;
            _uuid = uuid;
        }

        public string GetUUID()
        {
            return _uuid;
        }

        public int GetiOSMajorVersion()
        {
            var match = _versionRegex.Match(_buildVersion);
            if (!match.Success)
            {
                return -1;
            }

            int major;
            if (int.TryParse(match.Groups[1].Value, out major))
            {
                return major -4 ;
            }
            return -1;
        }
    }

}
