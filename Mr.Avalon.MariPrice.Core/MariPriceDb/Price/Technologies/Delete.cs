using System;
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
				public class Delete
				{
					[Bind("TechnologyUid")]
					public Guid TechnologyId { get; set; }

					
					[Bind("VersionId")]
					public int VersionId { get; set; }

					#region sql

					const string c_sql = @"
DELETE FROM  [MariPrice].[CompanyTechnology] WHERE VersionId = @VersionId AND TechnologyUid = @TechnologyUid
";
					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(c_sql, this);
					}
				}
			}
		}
	}
}
