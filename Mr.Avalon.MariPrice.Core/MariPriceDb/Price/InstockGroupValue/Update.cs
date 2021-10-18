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
			public partial class InstockGroupValue
			{
				[BindStruct]
				public class Update
				{
					[Bind("PriceGroupId")]
					public int PriceGroupId { get; set; }

					[Bind("WithNdsPrice")]
					public decimal? WithNdsPrice { get; set; }

					[Bind("WithoutNdsPrice")]
					public decimal? WithoutNdsPrice { get; set; }

					[Bind("WithNdsMarkup")]
					public decimal? WithNdsMarkup { get; set; }

					[Bind("WithoutNdsMarkup")]
					public decimal? WithoutNdsMarkup { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					public string[] UpdationList { get; set; }

					#region updateSql

					const string c_updateSql = @"
{update}
from
	[MariPrice].[PriceGroupInstockValue] as groups
where
	PriceGroupId=@PriceGroupId

-----

set @ResultCount=@@rowcount
";

					#endregion

					#region s_updationFields

					static HashSet<string> s_updationFields = new HashSet<string>(
						new[] {
							nameof(WithNdsPrice),
							nameof(WithoutNdsPrice),
							nameof(WithNdsMarkup),
							nameof(WithoutNdsMarkup)
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
