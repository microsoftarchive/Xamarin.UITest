using System.Collections.Generic;
using System.IO;

namespace Xamarin.UITest.Shared.Http
{
    public class BinaryResult
    {
        readonly FileInfo _file;
        readonly Dictionary<string, string> _headers;
		readonly int _statusCode;

		public BinaryResult(int statusCode, FileInfo file, Dictionary<string, string> headers)
        {
			_statusCode = statusCode;
            _file = file;
            _headers = headers;
        }

        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }

        public FileInfo File
        {
            get { return _file; }
        }

		public int StatusCode
		{
			get { return _statusCode; }
		}
	}
}