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
				public class ListWithVersion
				{
					public List<int> Ids { get; set; }

					public List<int> ClusterIds { get; set; }

					[Bind("Name")]
					public string Name { get; set; }

					[Bind("DisplayName")]
					public string DisplayName { get; set; }

					public ListWithVersion ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public ListWithVersion ForName(string names)
					{
						Name = names;
						return this;
					}

					public ListWithVersion ForDisplayName(string displayName)
					{
						DisplayName = displayName;
						return this;
					}

					public ListWithVersion ForClusters(params int[] clusterIds)
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
	gr.AdditionalLossPercentage,
	pcv.VersionId,
	pc.ActiveVersionId,
	pc.DraftVersionId	
FROM
	[MariPrice].[PriceGroup] as [gr]
	 JOIN [MariPrice].[PriceCluster] cl ON  cl.PriceClusterId=gr.PriceClusterId
	 JOIN MariPrice.PriceCompanyVersion AS pcv ON pcv.VersionId = cl.VersionId
	 JOIN MariPrice.PriceCompany AS pc ON pc.ActiveVersionId = pcv.VersionId or pc.ActiveVersionId = pcv.VersionId
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

					public List<GroupWithVersion> Exec(ISqlExecutor sql)
					{
						return sql.Query<GroupWithVersion>(GetQuery()).ToList(); 
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
