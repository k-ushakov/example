using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class ClusterSetting
			{
				public class List
				{
					public List<int> PriceClusterSettingIds { get; set; }

					public List<int> PriceClusterIds { get; set; }

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							PriceClusterSettingIds = ids.ToList();
						return this;
					}

					public List ForClusters(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							PriceClusterIds = clusterIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	clusterSetting.[PriceClusterSettingId],
	clusterSetting.[PriceClusterId],
	clusterSetting.[OrderMetalWeight],
	clusterSetting.[ProductionTime],
	clusterSetting.[Enabled]
FROM
	[MariPrice].[PriceClusterSetting] as [clusterSetting]
WHERE
	--{Ids - start}
	clusterSetting.[PriceClusterSettingId] in ({Ids}) and
	--{Ids - end}

	--{ClusterIds - start}
	clusterSetting.[PriceClusterId] in ({ClusterIds}) and
	--{ClusterIds - end}
	
	1=1
";
					#endregion

					public List<ClusterSetting> Exec(ISqlExecutor sql)
					{
						return sql.Query<ClusterSetting>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("Ids", PriceClusterSettingIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ClusterIds", PriceClusterIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
