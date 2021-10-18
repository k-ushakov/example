using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class Instock
			{
				[BindStruct]
				public class Import
				{
					[BindStruct("#TempBarcodes")]
					public class Item
					{
						[Bind("ProductId")]
						public int ProductId { get; set; }

						[NVarChar("Barcode", 200)]
						public string Barcode { get; set; }

						[Decimal("Weight", 18, 5)]
						public decimal Weight { get; set; }
					}

					[Bind("CompanyId")]
					public int CompanyId { get; set; }

					public List<Item> NewBarcodes { get; set; }

					#region c_setSql

					const string c_setSql = @"

DELETE barcode
FROM [MariPrice].Barcodes barcode
JOIN [MariPrice].Product product on barcode.ProductId = product.ProductId
WHERE product.CompanyId = @CompanyId

-----

INSERT INTO [MariPrice].Barcodes
(
	[ProductId],
	[Barcode],
	[Weight]
)
SELECT 
	temp.[ProductId], 
	temp.[Barcode], 
	temp.[Weight]
FROM {tableName}

";

					#endregion

					public void Exec(ISqlScopeExecutor m_sql)
					{
						if (NewBarcodes?.Any() == true)
						{
							var query = c_setSql;

							var tempSelect = GenerateSourceSelect(NewBarcodes);

							query = SqlQueriesFormater.ReplaceConst(query, "tableName", tempSelect);

							m_sql.Query(query, this);
						}
					}

					private string GenerateSourceSelect(List<Item> barcodes)
					{
						return $@"( VALUES {string.Join(", ",
							barcodes.Select(x =>
							$"({x.ProductId}, N'{x.Barcode}', {x.Weight.ToString(CultureInfo.InvariantCulture)})"))}) 
							temp (ProductId, Barcode, Weight)";
					}
				}
			}
		}
	}
}
