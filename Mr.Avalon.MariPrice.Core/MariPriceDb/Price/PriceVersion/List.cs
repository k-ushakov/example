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
			public partial class PriceVersion
			{
				public class List
				{
					public int[] VersionIds { get; set; }

					public List() { }
					public List(params int[] versionIds)
					{
						VersionIds = versionIds;
					}

					[BindStruct]
					public class Item
					{
						[Bind("VersionId")]
						public int VersionId { get; set; }

						[Bind("WithNdsPriceRequired")]
						public bool WithNdsPriceRequired { get; set; }
						[Bind("WithoutNdsPriceRequired")]
						public bool WithoutNdsPriceRequired { get; set; }
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	gv.VersionId,
	gv.WithNdsPriceRequired,
	gv.WithoutNdsPriceRequired
FROM
	[MariPrice].[PriceCompanyVersion] as [gv]

WHERE
	--{VersionIds - start}
	gv.[VersionId] in ({VersionIds}) and
	--{VersionIds - end}

	1=1
";

					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("VersionIds", VersionIds, x => string.Join(",", x)).Format(query);

						return sql.Query<Item>(query).ToList();
					}

				}
			}
		}
	}
}
