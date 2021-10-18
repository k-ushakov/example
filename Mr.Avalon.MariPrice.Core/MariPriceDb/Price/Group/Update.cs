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
			public partial class Group
			{
				[BindStruct]
				public class Update
				{
					[Bind("PriceGroupId")]
					public int Id { get; set; }

					[NVarChar("Name", 2000)]
					public string Name { get; set; }
					[NVarChar("DisplayName", 200)]
					public string DisplayName { get; set; }
					[NVarChar("ProductPublishInfo", 200)]
					public string ProductPublishInfo { get; set; }
					[Bind("LossPercentage")]
					public decimal LossPercentage { get; set; }
					[Bind("AdditionalLossPercentage")]
					public decimal? AdditionalLossPercentage { get; set; }
					[Bind("Enabled")]
					public bool Enabled { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					public string[] UpdationList { get; set; }

					#region updateSql

					const string c_updateSql = @"
{update}
from
	[MariPrice].[PriceGroup] as groups
where
	PriceGroupId=@PriceGroupId

-----

set @ResultCount=@@rowcount
";

					#endregion

					#region s_updationFields

					static HashSet<string> s_updationFields = new HashSet<string>(
						new[] {
						nameof(Name),
						nameof(DisplayName),
						nameof(ProductPublishInfo),
						nameof(LossPercentage),
						nameof(AdditionalLossPercentage),
						nameof(Enabled)
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
							throw new OutdatedTimestampApiException("The are no specific group");
					}

					private string GetQuery()
					{
						var query = c_updateSql;

						var updationList = UpdationList == null ?
							s_updationFields.ToArray() :
							UpdationList.Intersect(s_updationFields).ToArray();

						var updateFormator = new SqlUpdateQueryFormater(this, "groups")
							.AddUpdateList(updationList);

						query = SqlQueriesFormater.Update(query, "update", updateFormator);

						query = SqlQueriesFormater.RemoveAllNullSections(query, this);

						return query;
					}
				}
			}
		}
	}
}
