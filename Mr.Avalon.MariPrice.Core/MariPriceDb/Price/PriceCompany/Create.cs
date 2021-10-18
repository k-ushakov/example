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
				public class Create
				{
					[Bind("CompanyId")]
					public int CompanyId { get; set; }

					[Bind("ActiveVersionId")]
					public int ActiveVersionId { get; set; }
					[Bind("DraftVersionId")]
					public int DraftVersionId { get; set; }

					#region c_insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceCompany](
	[CompanyId], 
	[ActiveVersionId], 
	[DraftVersionId])
SELECT
	@CompanyId, 
	@ActiveVersionId, 
	@DraftVersionId
";
					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(c_insertSql, this);
					}
				}
			}
		}
	}
}
