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
			public partial class CompanyTechnology
			{
				[BindStruct]
				public class Update
				{
					[Bind("VersionId")]
					public int VersionId { get; set; }

					[Bind("TechnologyId")]
					public Guid TechnologyId { get; set; }

					[Bind("WithNdsPrice")]
					public decimal? WithNdsPrice { get; set; }
					[Bind("WithoutNdsPrice")]
					public decimal? WithoutNdsPrice { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					#region updateSql

					const string c_updateSql = @"
		{Update}
FROM 
[MariPrice].[CompanyTechnology] as tech
WHERE
	tech.[VersionId] = @VersionId
	AND 
	tech.[TechnologyUid] = @TechnologyId

-----

set @ResultCount=@@rowcount
";

					#endregion

					static HashSet<string> s_updationFields = new HashSet<string>(
					new[] {
							nameof(Update.WithNdsPrice),
							nameof(Update.WithoutNdsPrice),
						},
						StringComparer.InvariantCultureIgnoreCase);

					public void Exec(ISqlExecutor sql)
					{
						var query = c_updateSql;

						var updateFormator = new SqlUpdateQueryFormater(this, "tech")
							.AddUpdateList(s_updationFields.ToArray());

						query = SqlQueriesFormater.Update(query, "Update", updateFormator);

						sql.Query(query, this);

						if (ResultCount == 0)
							throw new OutdatedTimestampApiException("The are no specific tecknology");
					}
				}
			}
		}
	}
}
