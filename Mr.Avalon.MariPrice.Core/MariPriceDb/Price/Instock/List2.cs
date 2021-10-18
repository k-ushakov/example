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
			public partial class Instock
			{
				[BindStruct]
				public class List2
				{
					public List<int> CompanyIds { get; set; }

					public List<int> ProductIds { get; set; }

					[Bind("VersionId")]
					public int VersionId { get; set; }

					#region sql

					const string c_select = @"
SELECT 
	b.[ProductId],
	b.[Barcode],
	b.[Weight],
	p.[CompanyId],
	p.[ProductUid],
	p.[SizeUid],
	p.[Size],
	p.[Pn],
	p.[SizePn],
	p.WireThickness,
	gr.PriceGroupId,
	gr.Name
FROM MariPrice.[Barcodes] b
JOIN MariPrice.[Product] p on p.ProductId = b.ProductId
LEFT JOIN MariPrice.PriceGroupLink ln on p.ProductId = ln.ProductId and ln.VersionId=@VersionId
LEFT JOIN MariPrice.PriceGroup gr on ln.PriceGroupId =  gr.PriceGroupId

WHERE 
	--{CompanyIds - start}
	p.[CompanyId] in ({CompanyIds}) and
	--{CompanyIds - end}

	--{ProductIds - start}
	b.[ProductId] in ({ProductIds}) and
	--{ProductIds - end}

	1=1
";

					#endregion

					public List<InstockFull> Exec(ISqlFactory sql)
					{
						var query = c_select;

						if (CompanyIds?.Any() != true && ProductIds?.Any() != true)
							return new List<InstockFull>();

						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ProductIds", ProductIds, x => string.Join(",", x)).Format(query);

						return sql.Query<InstockFull>(query, this).ToList();
					}
				}
			}
		}
	}
}
