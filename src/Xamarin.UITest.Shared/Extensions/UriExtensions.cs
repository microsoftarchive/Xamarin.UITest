using System;

namespace Xamarin.UITest.Shared.Extensions
{
    public static class UriExtensions
	{
        public static Uri Combine(this Uri uri, string path, string queryString = null)
		{
			var builder = new UriBuilder (uri);

			if (uri.ToString().EndsWith ("/"))
			{
				builder.Path += path.TrimStart ('/');
			} 
			else
			{
				if (path.StartsWith ("/"))
				{
					builder.Path += path;
				} 
				else
				{
					builder.Path += "/" + path;
				}
			}

            if (!queryString.IsNullOrWhiteSpace())
            {
                builder.Query = queryString;
            }

			return builder.Uri;
		}
	}
}

