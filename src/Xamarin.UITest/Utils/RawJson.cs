namespace Xamarin.UITest.Utils
{
    /// <summary>
    /// Class for requesting the raw json from the framework.
    /// </summary>
    public class RawJson
    {
        private readonly string _json;

        /// <summary>
        /// Constructs an instance of the RawJson class.
        /// </summary>
        /// <param name="json">The json contents.</param>
        public RawJson(string json)
        {
            _json = json;
        }

        /// <summary>
        /// The contained json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }
    }
}