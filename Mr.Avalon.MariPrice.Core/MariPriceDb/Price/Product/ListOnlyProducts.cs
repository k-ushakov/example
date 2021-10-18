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
				public class ListOnlyProducts
				{
					[BindStruct]
					public class Item
					{
						[Bind("ProductId")]
						public int Id { get; set; }

						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }
						[Bind("SizeUid")]
						public Guid SizeUid { get; set; }
						[NVarChar("Name", 2000)]
						public string Name { get; set; }
						[NVarChar("Pn", 2000)]
						public string Pn { get; set; }
						[NVarChar("SizePn", 2000)]
						public string SizePn { get; set; }
						[Bind("CompanyId")]
						public int CompanyId { get; set; }
						[Bind("Status")]
						public int? Status { get; set; }
						[Bind("Enabled")]
						public bool Enabled { get; set; }
						[NVarChar("Metal", 200)]
						public string Metal { get; set; }
						[NVarChar("Size", 200)]
						public string Size { get; set; }
						[Decimal("WireThickness", 18, 2)]
						public decimal? WireThickness { get; set; }

						[NVarChar("SizeFullName", 1000)]
						public string SizeFullName { get; set; }
					}

					public List<int> CompanyIds { get; set; }

					public List<Guid> ProductUids { get; set; }


					public ListOnlyProducts ForCompanies(params int[] companies)
					{
						if (companies?.Any() == true)
							CompanyIds = companies.ToList();
						return this;
					}

					public ListOnlyProducts ForProducts(params Guid[] productUids)
					{
						if (productUids?.Any() == true)
							ProductUids = productUids.ToList();
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
	p.Metal
FROM
	[MariPrice].[Product] p

WHERE

	--{CompanyIds - start}
	p.[CompanyId] in ({CompanyIds}) and
	--{CompanyIds - end}

	--{ProductUids - start}
	p.[ProductUid] in ({ProductUids}) and
	--{ProductUids - end}

	1=1
";

					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						if (CompanyIds?.Any() == true || ProductUids?.Any() == true)
							return sql.Query<Item>(GetQuery()).ToList();
						else
							return new List<Item>();
					}

					string GetQuery()
					{
						
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ProductUids", ProductUids, x => string.Join(",", x.Select(g => $"'{g}'"))).Format(query);


						return query;
					}
				}
			}
		}
	}
}
