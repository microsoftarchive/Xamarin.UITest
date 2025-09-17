namespace Xamarin.UITest.Queries.PlatformSpecific
{
    internal class iOSResult
    {
		public string Id { get; set; }
		public string Class { get; set; }
		public string Label { get; set; }
		public string Description { get; set; }
		public AppRect Rect  { get; set; }
		public iOSFrame Frame  { get; set; }
        public bool Enabled { get; set; }
        public string Text { get; set; }
    }
}