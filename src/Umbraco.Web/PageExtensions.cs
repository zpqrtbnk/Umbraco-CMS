using System;
using System.Web.UI;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the page object
	/// </summary>
	public static class PageExtensions
	{
		/// <summary>
		/// Redirects to itself if the page is valid
		/// </summary>
		public static void RedirectToSelfIfValid(this Page page, object queryStrings, Action ifNotValid)
		{
			if (page.IsValid)
			{
				page.Response.RedirectToSelf(page.Request, queryStrings);
			}
			else
			{
				if (ifNotValid != null)
					ifNotValid();
			}
		}
	}
}