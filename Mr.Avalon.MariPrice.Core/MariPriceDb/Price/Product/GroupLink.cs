using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class Product
			{
				public partial class GroupLink
				{
					public List<Set> ForSet { get; set; }
					public List<Delete> ForDelete { get; set; }

					public class Delete
					{
						public int PriceGroupId { get; set; }
						public int ProductId { get; set; }
					}

					public class Set
					{
						public int PriceGroupId { get; set; }
						public int ProductId { get; set; }
						public int PriceClusterId { get; set; }
						public int VersionId { get; set; }
						public Guid ProductUid { get; set; }
					}

					#region updateSql

					const string c_updateSql = @"


--{Set - start}
UPDATE link SET link.UpdateTs = getutcdate()
FROM [MariPrice].[PriceGroupLink] as link
INNER JOIN {setSource} tempSet (PriceGroupId, ProductId, ProductUid, PriceClusterId)
ON 
	link.[PriceGroupId] = tempSet.[PriceGroupId]
	AND 
	link.[ProductId] = tempSet.[ProductId]

DELETE link
FROM [MariPrice].[PriceGroupLink] link
INNER JOIN {clusterIds} tempClusters (ProductUid, PriceClusterId,VersionId)
ON
	link.[ProductUid] = tempClusters.[ProductUid]
WHERE
	link.[PriceClusterId] <> tempClusters.[PriceClusterId]



UPDATE productCluster SET 
	productCluster.PriceClusterId = tempClusters.[PriceClusterId],
	productCluster.VersionId = tempClusters.[VersionId]
FROM [MariPrice].[ProductCluster] as productCluster  WITH (nolock)
INNER JOIN {clusterIds} tempClusters (ProductUid, PriceClusterId,VersionId)
ON 
	productCluster.[ProductUid] = tempClusters.[ProductUid]


INSERT INTO [MariPrice].[PriceGroupLink]
	([PriceGroupId], [ProductId], [ProductUid], [PriceClusterId], [UpdateTs])
SELECT 
	tempSet.[PriceGroupId], tempSet.[ProductId], tempSet.[ProductUid], tempSet.[PriceClusterId], getutcdate()
FROM {setSource} tempSet (PriceGroupId, ProductId, ProductUid, PriceClusterId)
LEFT JOIN [MariPrice].[PriceGroupLink] as link
ON 
	link.[PriceGroupId] = tempSet.[PriceGroupId]
	AND 
	link.[ProductId] = tempSet.[ProductId]
WHERE link.[ProductId] IS NULL

--{Set - end}


--{Delete - start}
DELETE link
FROM [MariPrice].[PriceGroupLink] link
INNER JOIN {deleteSource} tempDelete (PriceGroupId, ProductId)
ON
	link.[PriceGroupId] = tempDelete.[PriceGroupId]
	AND 
	link.[ProductId] = tempDelete.[ProductId]
--{Delete - end}
";

					#endregion

					public void Exec(ISqlScopeExecutor sql)
					{
						var query = c_updateSql;

						if (ForSet == null && ForDelete == null)
							return;

						if (ForSet == null)
							query = SqlQueriesFormater.RemoveSubString(query, "Set");
						else
						{
							var tempSet = GenerateSetSourceSelect(ForSet);
							var tempGroupClusters = GenerateClustersSourceSelect(ForSet);
							query = SqlQueriesFormater.ReplaceConst(query, "setSource", tempSet);
							query = SqlQueriesFormater.ReplaceConst(query, "clusterIds", tempGroupClusters);
						}

						if (ForDelete == null || !ForDelete.Any())
							query = SqlQueriesFormater.RemoveSubString(query, "Delete");
						else
						{
							var tempDelete = GenerateDeleteSourceSelect(ForDelete);
							query = SqlQueriesFormater.ReplaceConst(query, "deleteSource", tempDelete);
						}

						query = SqlQueriesFormater.RemoveLabels(query, "Delete", "Set");

						sql.Query(query);
					}

					private string GenerateClustersSourceSelect(List<Set> source)
					{
						return $@"( VALUES {string.Join(", ", source.GroupBy(x => x.ProductUid).Select(x => $"('{x.Key}', {x.First().PriceClusterId},{x.First().VersionId})"))})";
					}

					private string GenerateDeleteSourceSelect(List<Delete> source)
					{
						return $@"( VALUES {string.Join(", ", source.Select(x => $"({x.PriceGroupId}, {x.ProductId})"))})";
					}

					private string GenerateSetSourceSelect(List<Set> source)
					{
						return $@"( VALUES {string.Join(", ", source.Select(x => $"({x.PriceGroupId}, {x.ProductId}, '{x.ProductUid}', {x.PriceClusterId})"))})";
					}
				}
			}
		}
	}
}
