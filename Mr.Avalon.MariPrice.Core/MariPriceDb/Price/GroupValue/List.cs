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
			public partial class GroupValue
			{
				public class List
				{
					public List<int> Ids { get; set; }

					public List<int> GroupIds { get; set; }

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public List ForGroupIds(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							GroupIds = clusterIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	gr.PriceGroupValueId,
	gr.PriceGroupId,
	gr.PriceClusterSettingId,
	gr.WithNdsPrice,
	gr.WithoutNdsPrice,
	gr.WithNdsMarkup,
	gr.WithoutNdsMarkup
FROM
	[MariPrice].[PriceGroupValue] as [gr]

WHERE
	--{Ids - start}
	gr.[PriceGroupValueId] in ({Ids}) and
	--{Ids - end}

	--{GroupIds - start}
	gr.[PriceGroupId] in ({GroupIds}) and
	--{GroupIds - end}
	
	1=1
";
					#endregion

					public List<GroupValue> Exec(ISqlExecutor sql)
					{
						return sql.Query<GroupValue>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("Ids", Ids, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("GroupIds", GroupIds, x => string.Join(",", x)).Format(query);

						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
