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
			public partial class PriceVersion
			{
				[BindStruct]
				public class Update
				{
					[Bind("VersionId")]
					public int VersionId { get; set; }

					[Bind("WithNdsPriceRequired")]
					public bool WithNdsPriceRequired { get; set; }
					[Bind("WithoutNdsPriceRequired")]
					public bool WithoutNdsPriceRequired { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					public string[] UpdationList { get; set; }

					#region updateSql

					const string c_updateSql = @"
{update}
from
	[MariPrice].[PriceCompanyVersion] as compvers
where
	VersionId=@VersionId

-----

set @ResultCount=@@rowcount
";

					#endregion

					#region s_updationFields

					static HashSet<string> s_updationFields = new HashSet<string>(
						new[] {
						nameof(WithNdsPriceRequired),
						nameof(WithoutNdsPriceRequired)
						},
						StringComparer.InvariantCultureIgnoreCase);

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(GetQuery(), this);

						if (ResultCount == 0)
							throw new OutdatedTimestampApiException("The are no specific version");
					}

					private string GetQuery()
					{
						var query = c_updateSql;

						var updateFormator = new SqlUpdateQueryFormater(this, "compvers")
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
