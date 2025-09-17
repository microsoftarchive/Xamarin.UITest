using System.Net;

namespace Xamarin.UITest.XDB.Entities
{
	interface IHttpResult<T>
	{
		HttpStatusCode StatusCode { get; set; }

		T Content { get; set; } 
	}
}