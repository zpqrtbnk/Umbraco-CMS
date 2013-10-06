using System.Linq;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the PreValue object.
	/// </summary>
	public static class PreValueExtensions
	{
		/// <summary>
		/// Gets the alias of the specified PreValue
		/// </summary>
		/// <param name="preValue">The PreValue.</param>
		/// <returns>The alias</returns>
		public static string GetAlias(this PreValue preValue)
		{
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<DataTypePreValueDto>("WHERE id = @Id", new { Id = preValue.Id });

		    return dtos.Any() ? dtos.First().Alias : null;
		}
	}
}