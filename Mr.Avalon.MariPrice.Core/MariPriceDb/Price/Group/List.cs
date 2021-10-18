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
			public partial class Group
			{
				[BindStruct]
				public class List
				{
					public List<int> Ids { get; set; }

					public List<int> ClusterIds { get; set; }
					

					[Bind("Name")]
					public string Name { get; set; }
					
					[Bind("DisplayName")]
					public string DisplayName { get; set; }
			
					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public List ForName(string names)
					{
						Name = names;
						return this;
					}

					public List ForDisplayName(string displayName)
					{
						DisplayName = displayName;
						return this;
					}

					public List ForClusters(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							ClusterIds = clusterIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	gr.PriceGroupId,
	gr.PriceClusterId,
	gr.Name,
	gr.DisplayName,
	gr.ProductPublishInfo,
	gr.LossPercentage,
	isnull(gr.AdditionalLossPercentage,0.0) AdditionalLossPercentage
FROM
	[MariPrice].[PriceGroup] as [gr]	

WHERE
	--{GroupIds - start}
	gr.[PriceGroupId] in ({GroupIds}) and
	--{GroupIds - end}

	--{ClustersIds - start}
	gr.[PriceClusterId] in ({ClustersIds}) and
	--{ClustersIds - end}	

	--{Name - start}
	 isnull(gr.Name,'')+isnull(gr.DisplayName,'') Like N'%{Name}%' and
	--{Name - end}

	
	1=1
";
					#endregion

					public List<Group> Exec(ISqlExecutor sql)
					{
						return sql.Query<Group>(GetQuery(),this).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("GroupIds", Ids, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ClustersIds", ClusterIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("Name", Name, x => x).Format(query);					
						query = SqlQueriesFormater.RemoveLabels(query);
						return query;
					}
				}
			}
		}
	}
}
