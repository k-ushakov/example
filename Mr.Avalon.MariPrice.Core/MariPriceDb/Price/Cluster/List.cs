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
			public partial class Cluster
			{
				public class List
				{
					public List<int> ClusterIds { get; set; }

					public List<int> VersionIds { get; set; }

					public List ForClusterIds(params int[] ids)
					{
						if (ids?.Any() == true)
							ClusterIds = ids.ToList();
						return this;
					}

					public List ForVersionIds(params int[] versionIds)
					{
						if (versionIds?.Any() == true)
							VersionIds = versionIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	cluster.[PriceClusterId],
	cluster.[VersionId],
	cluster.[Name],
	cluster.[Enabled],
	cluster.[InStock],
	cluster.[InOrder],
	cluster.[Metal],
	cluster.[Quality]
FROM
	[MariPrice].[PriceCluster] as [cluster]
WHERE
	--{ClusterIds - start}
	cluster.[PriceClusterId] in ({ClusterIds}) and
	--{ClusterIds - end}

	--{VersionIds - start}
	cluster.[VersionId] in ({VersionIds}) and
	--{VersionIds - end}
	1=1
";
					#endregion

					public List<Cluster> Exec(ISqlExecutor sql)
					{
						return sql.Query<Cluster>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("ClusterIds", ClusterIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("VersionIds", VersionIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
