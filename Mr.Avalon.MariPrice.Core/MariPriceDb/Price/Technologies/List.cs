using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class CompanyTechnology
			{
				public class List
				{
					public List<int> VersionIds { get; set; }

					public List ForVersionId(params int[] versionIds)
					{
						if (versionIds?.Any() == true)
							VersionIds = versionIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	technology.VersionId,
	technology.TechnologyUid,
	technology.WithNdsPrice,
	technology.WithoutNdsPrice
FROM
	[MariPrice].[CompanyTechnology] as [technology]
WHERE
	--{VersionIds - start}
	technology.[VersionId] in ({VersionIds}) and
	--{VersionIds - end}
	
	1=1
";
					#endregion

					public List<CompanyTechnology> Exec(ISqlExecutor sql)
					{
						return sql.Query<CompanyTechnology>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("VersionIds", VersionIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
