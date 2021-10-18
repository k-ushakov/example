using Mr.Avalon.Common;
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
			public partial class Company
			{
				public class List
				{
					public int[] CompanyIds { get; set; }

					public List(params int[] companyIds)
					{
						CompanyIds = companyIds;
					}

					[BindStruct]
					public class Item
					{
						[Bind("CompanyId")]
						public int CompanyId { get; set; }

						[Bind("ActiveVersionId")]
						public int ActiveVersionId { get; set; }
						[Bind("DraftVersionId")]
						public int DraftVersionId { get; set; }
					}

					#region c_sql

					const string c_sql = @"
SELECT
	comp.[CompanyId],
	comp.[ActiveVersionId], 
	comp.[DraftVersionId]

FROM
	[MariPrice].[PriceCompany] as comp

WHERE
	--{CompanyIds - start}
	comp.CompanyId in ({CompanyIds}) and
	--{CompanyIds - end}
	1=1
";
					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						var query = c_sql;

						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);

						return sql.Query<Item>(query)?.ToList();
					}
				}
			}
		}
	}
}
