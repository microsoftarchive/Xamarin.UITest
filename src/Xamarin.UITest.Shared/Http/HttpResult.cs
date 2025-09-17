using System;

namespace Xamarin.UITest.Shared.Http
{
    public class HttpResult
	{
		readonly int _statusCode;
		readonly string _contents;

		public HttpResult (int statusCode, string contents)
		{
			_contents = contents;
			_statusCode = statusCode;
		}

		public int StatusCode
		{
			get { return _statusCode; }
		}

		public string Contents 
		{
			get { return _contents; }
		}

        public override string ToString()
        {
            return string.Format("Status: {0}{1}Contents:{1}{2}", StatusCode, Environment.NewLine, Contents);
        }
	}
}