using System;
using System.Collections.Generic;
using System.Text;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class ClusterSetting
			{
				[BindStruct]
				public class Delete
				{
					[Bind("PriceClusterId")]
					public int ClusterId { get; set; }

					public List<int> PriceClusterSettingIds { get; set; }

					#region insertSql

					const string c_sql = @"
DELETE FROM  
[MariPrice].[PriceClusterSetting] 
WHERE 
	PriceClusterId = @PriceClusterId  AND

	--{PriceClusterSettingIds - start}
	[PriceClusterSettingId] in ({PriceClusterSettingIds}) and
	--{PriceClusterSettingIds - end}

1=1
";
					#endregion

					public void Exec(ISqlExecutor sql)
					{
						var query = c_sql;
						query = SqlQueriesFormater.RemoveOrReplace("PriceClusterSettingIds", PriceClusterSettingIds, x => string.Join(",", x)).Format(query);
						query = SqlQueriesFormater.RemoveLabels(query);
						sql.Query(query, this);
					}
				}
			}
		}
	}
}
