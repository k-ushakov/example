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
			public partial class Company
			{
				[BindStruct]
				public class ListByVersion
				{
					[Bind("VersionId")]
					public int VersionId { get; set; }

					public ListByVersion(int versionId)
					{
						VersionId = versionId;
					}

					[BindStruct]
					public class Item
					{
						[Bind("CompanyId")]
						public int CompanyId { get; set; }

						[Bind("ActiveVersionId")]
						public int ActiveVersionId { get; set; }
						[Bind("DraftVersionId")]
						public int DraftVersionId { get; set; }
					}

					#region c_sql

					const string c_sql = @"
SELECT
	comp.[CompanyId],
	comp.[ActiveVersionId], 
	comp.[DraftVersionId]

FROM
	[MariPrice].[PriceCompany] as comp

WHERE	comp.ActiveVersionId  = @VersionId OR comp.DraftVersionId =  @VersionId
";
					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						return sql.Query<Item>(c_sql, this)?.ToList();
					}
				}
			}
		}
	}
}
