using Newtonsoft.Json;
using System.Collections.Generic;

namespace Xamarin.UITest.XDB.Services.OSX.IDB
{
	internal class iOSAppInfo
	{
		[JsonProperty("bundle_id")]
		public string BundleId;

		[JsonProperty("name")]
		public string Name;

		[JsonProperty("install_type")]
		public string InstallType;

		[JsonProperty("architectures")]
		public List<string> Architectures;

		[JsonProperty("process_state")]
		public string ProcessState;

		[JsonProperty("debuggable")]
		public bool Debuggable;

		[JsonProperty("pid")]
		public string PID;
	}
}

