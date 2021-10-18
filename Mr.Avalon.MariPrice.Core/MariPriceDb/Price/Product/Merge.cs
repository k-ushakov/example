using Mr.Avalon.Spec.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
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
				public class Merge
				{
					public Item[] Products { get; set; }

					[BindStruct]
					public partial class Item
					{
						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }

						[Bind("SizeUid")]
						public Guid SizeUid { get; set; }

						[NVarChar("Title", 2000)]
						public string Title { get; set; }
						[NVarChar("Pn", 2000)]
						public string Pn { get; set; }
						[NVarChar("SizePn", 2000)]
						public string SizePn { get; set; }
						[NVarChar("Metal", 2000)]
						public string Metal { get; set; }

						[NVarChar("SizeFullName", 1000)]
						public string SizeFullName { get; set; }

						[Bind("Status")]
						public ProductState Status { get; set; }

						[NVarChar("ImageUrl", 2000)]
						public string ImageUrl { get; set; }
						[NVarChar("SearchField", 2000)]
						public string SearchField { get; set; }
						[NVarChar("DataJson", 0, MaxSize = true)]
						public string DataJson { get; set; }

						[Bind("CompanyId")]
						public int CompanyId { get; set; }

						[Bind("Enabled")]
						public bool Enabled { get; set; }
						[NVarChar("Name", 2000)]
						public string Name { get; set; }

						[NVarChar("Size", 200)]
						public string Size { get; set; }
						[Decimal("WireThickness", 18, 2)]
						public decimal? WireThickness { get; set; }
					}

					#region updateSql

					const string c_updateSql = @"
		{Update}
FROM [MariPrice].[Product] as product 
INNER JOIN {tableName} 
ON 
	product.[ProductUid] = temp.[ProductUid]
	AND 
	product.[SizeUid] = temp.[SizeUid]

INSERT INTO [MariPrice].[Product]
	([ProductUid], [SizeUid], [Name], [CompanyId], [Enabled],Title,Pn,SizePn,Metal,Status,ImageUrl,SearchField,DataJson, [SizeFullName], [Size], [WireThickness])
SELECT 
	temp.[ProductUid], temp.[SizeUid], temp.[Name], temp.[CompanyId], 1, temp.Title, temp.Pn, temp.SizePn, temp.Metal, temp.Status, temp.ImageUrl, temp.SearchField, temp.DataJson, temp.[SizeFullName], temp.[Size], temp.[WireThickness]
FROM {tableName} 
LEFT JOIN [MariPrice].[Product] as product 
ON 
	product.[ProductUid] = temp.[ProductUid]
	AND 
	product.[SizeUid] = temp.[SizeUid]
WHERE product.[ProductId] IS NULL

UPDATE product SET product.[Enabled] = 0
FROM [MariPrice].[Product] product 
LEFT JOIN {tableName} 
ON
	product.[ProductUid] = temp.[ProductUid]
	AND 
	product.[SizeUid] = temp.[SizeUid]
WHERE temp.[ProductUid] IS NULL
AND product.[ProductUid] in ({ProductUids})
";

					#endregion

					static HashSet<string> s_updationFields = new HashSet<string>(
					new[] {
							nameof(Item.Enabled),
							nameof(Item.Name),
							nameof(Item.CompanyId),
							nameof(Item.Title),
							nameof(Item.Pn),
							nameof(Item.SizePn),
							nameof(Item.Metal),
							nameof(Item.Status),
							nameof(Item.ImageUrl),
							nameof(Item.SearchField),
							nameof(Item.DataJson),
							nameof(Item.SizeFullName),
							nameof(Item.Size),
							nameof(Item.WireThickness),
						},
						StringComparer.InvariantCultureIgnoreCase);

					public void Exec(ISqlExecutor sql)
					{
						var query = c_updateSql;

						if (Products == null || !Products.Any())
							return;
						
						var tempSelect = GenerateSourceSelect(Products);

						query = SqlQueriesFormater.ReplaceConst(query, "tableName", tempSelect);

						var updateFormator = new SqlJoinedUpdateQueryFormater<Item>("temp", "product")
							.AddUpdateList(s_updationFields.ToArray());

						query = SqlQueriesFormater.Update(query, "Update", updateFormator);
						
						query = SqlQueriesFormater.RemoveOrReplace("ProductUids", Products,
							x => string.Join(",", x.Select(g => $"'{g.ProductUid}'").Distinct())).Format(query);

						sql.Query(query);
					}


					private string GenerateSourceSelect(Item[] products)
					{
						return $@"( VALUES {string.Join(", ",
							products.Select(x =>
							$"('{x.ProductUid}', '{x.SizeUid}', N'{x.Name}', {x.CompanyId}, {(x.Enabled ? "1" : "0")}, N'{x.Title}',N'{x.Pn}',N'{x.SizePn}',N'{x.Metal}',{(int)x.Status},N'{x.ImageUrl}',N'{x.SearchField}',N'{x.DataJson}', N'{x.SizeFullName}', N'{x.Size}', {(x.WireThickness.HasValue ? x.WireThickness.Value.ToString("G", CultureInfo.InvariantCulture) : "NULL" )} )"))}) 
							temp (ProductUid, SizeUid, Name, CompanyId, Enabled, Title, Pn,SizePn, Metal, Status, ImageUrl, SearchField, DataJson, SizeFullName, Size, WireThickness)";
					}
				}
			}
		}
	}
}
