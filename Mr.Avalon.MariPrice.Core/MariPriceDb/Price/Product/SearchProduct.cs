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
				public class SearchProduct
				{
					public Guid[] ProductUids { get; set; }

					[Bind("VersionId")]
					public int VersionId { get; set; }

					#region c_sql
					const string c_sql = @"
SELECT 
	p.Name,
	p.ProductId,
	p.SizeUid,
	p.ProductUid,

	p.Title,
	p.SizePn as Pn,
	p.Metal,
	p.Status,
	p.ImageUrl,
	p.Size,
	p.WireThickness,

    pgl.PriceGroupId,
	pro_cl.PriceClusterId

FROM [MariPrice].[Product] p WITH (nolock)
LEFT JOIN [MariPrice].[PriceGroupLink] pgl on p.ProductId = pgl.ProductId  and pgl.VersionId = @VersionId
LEFT JOIN [MariPrice].[ProductCluster] pro_cl on p.ProductUid = pro_cl.ProductUid  and pro_cl.VersionId = @VersionId


WHERE
	
	--{ProductUids - start}
	p.[ProductUid] in ({ProductUids}) and
	--{ProductUids - end}

	p.Enabled = 1  and

	1=1
";
					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						return sql.Query<Item>(GetQuery(), this).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;

						var products = ProductUids.Select(x => $"'{x}'").ToArray();
						query = SqlQueriesFormater.RemoveOrReplace("ProductUids", products, x => string.Join(",", x)).Format(query);

						return query;
					}

					[BindStruct]
					public class Item
					{
						[Bind("ProductId")]
						public int Id { get; set; }
						[NVarChar("Name", 2000)]
						public string Name { get; set; }
						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }
						[Bind("SizeUid")]
						public Guid SizeUid { get; set; }

						[NVarChar("Title", 2000)]
						public string Title { get; set; }
						[NVarChar("Pn", 2000)]
						public string Pn { get; set; }
						[NVarChar("Metal", 2000)]
						public string Metal { get; set; }
						[Bind("Status")]
						public int? Status { get; set; }
						[NVarChar("ImageUrl", 2000)]
						public string ImageUrl { get; set; }

						[NVarChar("Size", 200)]
						public string Size { get; set; }
						[Decimal("WireThickness", 18, 2)]
						public decimal? WireThickness { get; set; }

						[Bind("PriceGroupId")]
						public int? PriceGroupId { get; set; }
						[Bind("PriceClusterId")]
						public int? ClusterId { get; set; }
					}
				}
			}
		}
	}
}
