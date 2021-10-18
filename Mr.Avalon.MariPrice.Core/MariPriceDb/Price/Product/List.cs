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
				public class List
				{
					public List<int> Ids { get; set; }

					public List<Guid> ProductUids { get; set; }

					public List<Guid> SizeUids { get; set; }

					public List<int> PriceGroupIds { get; set; }

					public List<int> CompanyIds { get; set; }

					public List<string> Pns { get; set; }

					public List<string> Sizes { get; set; }

					public List<decimal> WireThickness { get; set; }

					public bool? OnlyEnabled { get; set; }

					public bool? WireThicknessIsNull { get; set; }

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public List ForProducts(params Guid[] productUids)
					{
						if (productUids?.Any() == true)
							ProductUids = productUids.ToList();
						return this;
					}

					public List ForSizes(params Guid[] sizeUids)
					{
						if (sizeUids?.Any() == true)
							SizeUids = sizeUids.ToList();
						return this;
					}

					public List ForPriceGroups(params int[] priceGroups)
					{
						if (priceGroups?.Any() == true)
							PriceGroupIds = priceGroups.ToList();
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
	[MariPrice].[Product] as [p] WITH (nolock)
LEFT JOIN 
	[MariPrice].[PriceGroupLink] link on p.ProductId = link.ProductId

WHERE
	--{Ids - start}
	p.[ProductId] in ({Ids}) and
	--{Ids - end}

	--{ProductUids - start}
	p.[ProductUid] in ({ProductUids}) and
	--{ProductUids - end}

	--{Pns - start}
	p.[Pn] in ({Pns}) and
	--{Pns - end}

	--{CompanyIds - start}
	p.[CompanyId] in ({CompanyIds}) and
	--{CompanyIds - end}

	--{SizeUids - start}
	p.[SizeUid] in ({SizeUids}) and
	--{SizeUids - end}

	--{Sizes - start}
	p.[Size] in ({Sizes}) and
	--{Sizes - end}

	--{WireThickness - start}
	p.[WireThickness] in ({WireThickness}) and
	--{WireThickness - end}

	--{WireThicknessIsNull - start}
	p.[WireThickness] IS NULL and
	--{WireThicknessIsNull - end}

	--{PriceGroupIds - start}
	link.[PriceGroupId] in ({PriceGroupIds}) and
	--{PriceGroupIds - end}

	--{OnlyEnabled - start}
	p.Enabled=1 and
	--{OnlyEnabled - end}

	1=1
";

					#endregion

					public List<Product> Exec(ISqlExecutor sql)
					{
						return sql.Query<Product>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("Ids", Ids, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ProductUids", ProductUids, x => string.Join(",", x.Select(g => $"'{g}'"))).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("PriceGroupIds", PriceGroupIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("Pns", Pns, x => string.Join(",", x.Select(i => $"N'{i}'"))).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("Sizes", Sizes, x => string.Join(",", x.Select(i => $"N'{i}'"))).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("WireThickness", WireThickness, x => string.Join(",", x.Select(i => $"{i}"))).Format(query);
						if (SizeUids?.Any() == true)
							query = SqlQueriesFormater.RemoveOrReplace("SizeUids", SizeUids.Append(Guid.Empty), x => string.Join(",", x.Select(g => $"'{g}'"))).Format(query);
						else
							query = SqlQueriesFormater.RemoveSubString(query, "SizeUids");

						if (!(OnlyEnabled.HasValue && OnlyEnabled.Value))
							query = SqlQueriesFormater.RemoveSubString(query, "OnlyEnabled");
						else
							query = SqlQueriesFormater.RemoveLabels(query, "OnlyEnabled");

						if (!(WireThicknessIsNull.HasValue && WireThicknessIsNull.Value))
							query = SqlQueriesFormater.RemoveSubString(query, "WireThicknessIsNull");
						else
							query = SqlQueriesFormater.RemoveLabels(query, "WireThicknessIsNull");


						return query;
					}
				}
			}
		}
	}
}
