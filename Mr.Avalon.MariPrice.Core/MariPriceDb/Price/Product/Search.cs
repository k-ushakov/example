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
				public class Search
				{
					[Bind("CompanyId")]
					public int CompanyId { get; set; }

					[Bind("VersionId")]
					public int VersionId { get; set; }

					public string Name { get; set; }

					public List<int> PriceGroupIds { get; set; }

					public int? Start { get; set; }
					public int? Count { get; set; }

					public Search ForCompany(int companyId)
					{
						CompanyId = companyId;
						return this;
					}

					public Search WithNameLike(string name)
					{
						Name = name;
						return this;
					}

					#region c_sql
					const string c_sql = @"
SELECT 
	{topPaging}
	p.ProductUid,
	COUNT(*) OVER() as Total

FROM [MariPrice].[Product] p WITH (nolock)
LEFT JOIN [MariPrice].[PriceGroupLink] pgl on p.ProductId = pgl.ProductId and pgl.VersionId = @VersionId
LEFT JOIN [MariPrice].[ProductCluster] pro_cl on p.ProductUid = pro_cl.ProductUid  and pro_cl.VersionId = @VersionId


WHERE
	p.[CompanyId] = @CompanyId and

	--{Name - start}
	p.[SearchField] Like N'%{Name}%' and
	--{Name - end}
	
	--{PriceGroupIds - start}
	pgl.[PriceGroupId] in ({PriceGroupIds}) and
	--{PriceGroupIds - end}

	{sizeCondition} 
	

	p.Enabled = 1

order by p.ProductId

{offsetPaging}

";
					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						return sql.Query<Item>(GetQuery(), this).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;

						query = SqlQueriesFormater.Page("topPaging", "offsetPaging")
							.Top(Count).Offset(Start)
							.Format(query);

						query = SqlQueriesFormater.RemoveOrReplace("Name", Name, x => x).Format(query);

						query = SqlQueriesFormater.RemoveOrReplace("PriceGroupIds", PriceGroupIds, x => string.Join(",", x)).Format(query);

						if (PriceGroupIds?.Any() == true)
							query = SqlQueriesFormater.ReplaceConst(query, "sizeCondition", "");
						else
							query = SqlQueriesFormater.ReplaceConst(query, "sizeCondition", "p.SizeUid='00000000-0000-0000-0000-000000000000'  AND");

						return query;
					}

					[BindStruct]
					public class Item
					{
						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }
						[Bind("Total")]
						public int TotalCount { get; set; }
					}
				}
			}
		}
	}
}
