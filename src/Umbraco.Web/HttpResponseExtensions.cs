using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the httpResponse object
	/// </summary>
	public static class HttpResponseExtensions
	{
		/// <summary>
		/// Redirects the current page to itself
		/// </summary>
		/// <param name="response"></param>
		/// <param name="request"></param>
		/// <param name="queryStrings"></param>
		public static void RedirectToSelf(this HttpResponseBase response, HttpRequestBase request, object queryStrings = null)
		{
			var d = queryStrings.ToDictionary<object>();
			var url = request.RawUrl.Contains("?")
				          ? request.RawUrl.EnsureEndsWith('&')
				          : request.RawUrl.EnsureEndsWith('?');
			response.Redirect(url + d.ToQueryString());
		}

		/// <summary>
		/// Redirects the current page to itself
		/// </summary>
		/// <param name="response"></param>
		/// <param name="request"></param>
		/// <param name="queryStrings"></param>
		public static void RedirectToSelf(this HttpResponse response, HttpRequest request, object queryStrings = null)
		{
			new HttpResponseWrapper(response).RedirectToSelf(new HttpRequestWrapper(request), queryStrings);
		}
	}
}