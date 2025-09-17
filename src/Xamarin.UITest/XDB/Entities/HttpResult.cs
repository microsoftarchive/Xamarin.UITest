using System.Net;

namespace Xamarin.UITest.XDB.Entities
{
	class HttpResult<T> : IHttpResult<T>
	{
		public HttpStatusCode StatusCode { get; set; }

		public T Content { get; set; } 
	}
}