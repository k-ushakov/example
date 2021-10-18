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
			public partial class Product
			{
				[BindStruct]
				public class ListOnlyActive
				{
					public List<int> Ids { get; set; }

					public List<Guid> ProductUids { get; set; }

					public List<Guid> SizeUids { get; set; }

					public List<int> PriceGroupIds { get; set; }

					public List<int> ClusterIds { get; set; }

					public List<string> Pns { get; set; }

					public List<int> CompanyIds { get; set; }

					public bool OnlyActiveCluster { get; set; }

					[Bind("Enable")]
					public int Enable { get; set; }

					public ListOnlyActive ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public ListOnlyActive ForProducts(params Guid[] productUids)
					{
						if (productUids?.Any() == true)
							ProductUids = productUids.ToList();
						return this;
					}

					public ListOnlyActive ForSizes(params Guid[] sizeUids)
					{
						if (sizeUids?.Any() == true)
							SizeUids = sizeUids.ToList();
						return this;
					}

					public ListOnlyActive ForPriceGroups(params int[] priceGroups)
					{
						if (priceGroups?.Any() == true)
							PriceGroupIds = priceGroups.ToList();
						return this;
					}

					public ListOnlyActive ForClusters(params int[] clusters)
					{
						if (clusters?.Any() == true)
							ClusterIds = clusters.ToList();
						return this;
					}

					public ListOnlyActive ForPns(params string[] pns)
					{
						if (pns?.Any() == true)
							Pns = pns.ToList();
						return this;
					}

					public ListOnlyActive ForCompanies(params int[] companyIds)
					{
						if (companyIds?.Any() == true)
							CompanyIds = companyIds.ToList();
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	p.ProductId,
	p.ProductUid,
	p.SizeUid,
	p.Name,
	p.Pn,
	p.SizePn,
	p.CompanyId,
	p.Status,
	p.Enabled,
	p.SizeFullName,
	p.Size,
	p.WireThickness,
	p.Metal,
	link.PriceGroupId
FROM
	[MariPrice].[Product] as [p]
		INNER JOIN 	[MariPrice].[PriceGroupLink] link on p.ProductId = link.ProductId
		INNER JOIN	[MariPrice].PriceGroup pg on pg.PriceGroupId =  link.PriceGroupId
		INNER JOIN   [MariPrice].PriceCluster pl on  pl.PriceClusterId = pg.PriceClusterId
		INNER JOIN   [MariPrice].PriceCompany pc on  pc.ActiveVersionId = pl.VersionId

WHERE

	--{Ids - start}
	p.[ProductId] in ({Ids}) and
	--{Ids - end}

	--{ProductUids - start}
	p.[ProductUid] in ({ProductUids}) and
	--{ProductUids - end}

	--{SizeUids - start}
	p.[SizeUid] in ({SizeUids}) and
	--{SizeUids - end}

	--{PriceGroupIds - start}
	link.[PriceGroupId] in ({PriceGroupIds}) and
	--{PriceGroupIds - end}

	--{ClusterIds - start}
	pl.[PriceClusterId] in ({ClusterIds}) and
	--{ClusterIds - end}

	--{Pns - start}
	p.[Pn] in ({Pns}) and
	--{Pns - end}

	--{CompanyIds - start}
	p.[CompanyId] in ({CompanyIds}) and
	--{CompanyIds - end}
	
	--{OnlyActiveCluster - start}
	pl.Enabled=@Enable and
	--{OnlyActiveCluster - end}
	
	1=1
";

					#endregion

					public List<Product> Exec(ISqlExecutor sql)
					{
						return sql.Query<Product>(GetQuery(), this).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("Pns", Pns, x => string.Join(",", x.Select(g => $"N'{g}'"))).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("Ids", Ids, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ClusterIds", ClusterIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ProductUids", ProductUids, x => string.Join(",", x.Select(g => $"'{g}'"))).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("PriceGroupIds", PriceGroupIds, x => string.Join(",", x)).Format(query);

						if (SizeUids?.Any() == true)
							query = SqlQueriesFormater.RemoveOrReplace("SizeUids", SizeUids.Append(Guid.Empty), x => string.Join(",", x.Select(g => $"'{g}'"))).Format(query);

						else
							query = SqlQueriesFormater.RemoveSubString(query, "SizeUids");

						if (!OnlyActiveCluster)

							Enable = 0;
						//query = SqlQueriesFormater.RemoveSubString(query, "OnlyActiveCluster");

						else

							Enable = 1;




						return query;
					}
				}
			}
		}
	}
}
