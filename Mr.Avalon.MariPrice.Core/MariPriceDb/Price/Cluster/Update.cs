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
			public partial class Cluster
			{
				[BindStruct]
				public class Update
				{
					[Bind("PriceClusterId")]
					public int Id { get; set; }

					[Bind("Name")]
					public Guid Name { get; set; }

					[Bind("Enabled")]
					public bool Enabled { get; set; }

					[Bind("InStock")]
					public bool InStock { get; set; }

					[Bind("InOrder")]
					public bool InOrder { get; set; }

					[Bind("Metal")]
					public Guid? Metal { get; set; }

					[Bind("Quality")]
					public Guid? Quality { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					public string[] UpdationList { get; set; }

					#region updateSql

					const string c_updateSql = @"
{update}
from
	[MariPrice].[PriceCluster] as clusters
where
	PriceClusterId=@PriceClusterId

-----

set @ResultCount=@@rowcount
";

					#endregion

					#region s_updationFields

					static HashSet<string> s_updationFields = new HashSet<string>(
						new[] {
						nameof(Name),
						nameof(Enabled),
						nameof(InStock),
						nameof(InOrder),
						nameof(Metal),
						nameof(Quality)
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

						var updateFormator = new SqlUpdateQueryFormater(this, "clusters")
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
