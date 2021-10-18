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
			public partial class Instock
			{
				public class List
				{
					public List<int> CompanyIds { get; set; }

					public List<int> ProductIds { get; set; }

					#region sql

					const string c_select = @"
SELECT 
	b.[ProductId],
	b.[Barcode],
	b.[Weight],
	p.[CompanyId],
	p.[ProductUid],
	p.[SizeUid]
FROM MariPrice.[Barcodes] b
JOIN MariPrice.[Product] p on p.ProductId = b.ProductId

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

					public List<Instock> Exec(ISqlFactory sql)
					{
						var query = c_select;

						if (CompanyIds?.Any() != true && ProductIds?.Any() != true)
							return new List<Instock>();

						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveOrReplace("ProductIds", ProductIds, x => string.Join(",", x)).Format(query);

						return sql.Query<Instock>(query).ToList();
					}
				}
			}
		}
	}
}
