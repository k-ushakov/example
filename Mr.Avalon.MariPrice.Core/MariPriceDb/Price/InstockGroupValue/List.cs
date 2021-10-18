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
			public partial class InstockGroupValue
			{
				public class List
				{
					public List<int> GroupIds { get; set; }

					public List ForGroupIds(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							GroupIds = clusterIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	gr.PriceGroupId,
	gr.WithNdsPrice,
	gr.WithoutNdsPrice,
	gr.WithNdsMarkup,
	gr.WithoutNdsMarkup
FROM
	[MariPrice].[PriceGroupInstockValue] as [gr]

WHERE
	--{GroupIds - start}
	gr.[PriceGroupId] in ({GroupIds}) and
	--{GroupIds - end}
	
	1=1
";
					#endregion

					public List<InstockGroupValue> Exec(ISqlExecutor sql)
					{
						return sql.Query<InstockGroupValue>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("GroupIds", GroupIds, x => string.Join(",", x)).Format(query);

						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
