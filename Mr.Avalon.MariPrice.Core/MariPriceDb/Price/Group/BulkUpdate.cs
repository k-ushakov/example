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
				public class BulkUpdate
				{
					public class Item
					{
						public bool Enabled { get; set; }
						public string Variants { get; set; }
					}
					public Dictionary<int, Item> VariantsForGroups { get; set; }

					#region updateSql

					const string c_updateSql = @"

UPDATE groups 
SET 
	groups.Variants = updatingValues.variants,
	groups.Enabled = updatingValues.enabled
FROM [MariPrice].[PriceGroup] groups
JOIN 
(
	Select groupId, variants, enabled From ( Values {Values} ) t(groupId, variants, enabled) 
) updatingValues on groups.PriceGroupId = updatingValues.groupId

";

					#endregion


					public void Exec(ISqlExecutor sql)
					{
						if (VariantsForGroups?.Any() == true)
							sql.Query(GetQuery());
					}

					private string GetQuery()
					{
						var query = c_updateSql;

						query = SqlQueriesFormater.ReplaceConst(query, "Values",
							string.Join(", ", VariantsForGroups.Select(x => $"({x.Key}, '{x.Value.Variants}', {(x.Value.Enabled ? "1" : "0")})")));

						return query;
					}
				}
			}
		}
	}
}
