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
			public partial class Group
			{
				public class Search
				{
					public List<int> CompanyIds { get; set; } = new List<int>();

					public string Name { get; set; }

					public int MaxCount { get; set; }

					#region c_sql
					const string c_sql = @"
SELECT 
	{top}
	[group].PriceGroupId,
	[group].Name
FROM
	[MariPrice].[PriceGroup] as [group]
JOIN
	[MariPrice].[PriceCluster] as [cluster] on [group].PriceClusterId = cluster.PriceClusterId
JOIN 
	[MariPrice].[PriceCompany] as [company] on [cluster].VersionId = company.ActiveVersionId
WHERE
	--{CompanyIds - start}
	company.[CompanyId] in ({CompanyIds}) and
	--{CompanyIds - end}

	--{Name - start}
	[group].[Name] Like N'%{Name}%' and
	--{Name - end}
";
					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						return sql.Query<Item>(GetQuery()).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;

						query = SqlQueriesFormater.RemoveOrReplace("CompanyIds", CompanyIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.ReplaceConst(query, "top", $"TOP {MaxCount}");
						query = SqlQueriesFormater.RemoveOrReplace("Name", Name, x => x).Format(query);
						return query;
					}

					public Search ForOwners(params int[] companyIds)
					{
						if (companyIds?.Any() == true)
							CompanyIds = companyIds.ToList();
						return this;
					}

					public Search WithNameLike(string groupName)
					{
						if (!string.IsNullOrWhiteSpace(groupName))
							Name = groupName;
						return this;
					}

					public Search TakeOnly(int count)
					{
						MaxCount = count;
						return this;
					}

					[BindStruct]
					public partial class Item
					{
						[Bind("PriceGroupId")]
						public int Id { get; set; }
						[Bind("Name")]
						public string Name { get; set; }
					}
				}
			}
		}
	}
}
