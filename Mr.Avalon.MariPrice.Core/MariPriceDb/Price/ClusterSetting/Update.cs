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
			public partial class ClusterSetting
			{
				[BindStruct]
				public class Update
				{
					[Bind("PriceClusterSettingId")]
					public int Id { get; set; }

					[Bind("PriceClusterId")]
					public int ClusterId { get; set; }

					[Bind("OrderMetalWeight")]
					public decimal OrderMetalWeight { get; set; }

					[Bind("ProductionTime")]
					public int ProductionTime { get; set; }

					[Bind("Enabled")]
					public bool Enabled { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					public string[] UpdationList { get; set; }

					#region updateSql

					const string c_updateSql = @"
{update}
from
	[MariPrice].[PriceClusterSetting] as clustersetting
where
	PriceClusterSettingId=@PriceClusterSettingId

-----

set @ResultCount=@@rowcount
";

					#endregion

					#region s_updationFields

					static HashSet<string> s_updationFields = new HashSet<string>(
						new[] {
						nameof(OrderMetalWeight),
						nameof(ProductionTime)
						},
						StringComparer.InvariantCultureIgnoreCase);

					#endregion

					public Update DefaultUpdationList()
					{
						UpdationList = s_updationFields.ToArray();
						return this;
					}

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(GetQuery(), this);

						if (ResultCount == 0)
							throw new OutdatedTimestampApiException("The are no specific cluster");
					}

					private string GetQuery()
					{
						var query = c_updateSql;

						var updateFormator = new SqlUpdateQueryFormater(this, "clustersetting")
							.AddUpdateList(UpdationList.Intersect(s_updationFields).ToArray());

						query = SqlQueriesFormater.Update(query, "update", updateFormator);

						query = SqlQueriesFormater.RemoveAllNullSections(query, this);

						return query;
					}
				}
			}
		}
	}
}
