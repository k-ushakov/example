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
				public class Swap
				{
					[Bind("CompanyId")]
					public int CompanyId { get; set; }

					[Bind("ResultCount", Direction = System.Data.ParameterDirection.Output)]
					public int ResultCount { get; set; }

					#region updateSql

					const string c_updateSql = @"
UPDATE [MariPrice].[PriceCompany] 
	SET 
		ActiveVersionId = DraftVersionId, 
		DraftVersionId = ActiveVersionId 
WHERE
	CompanyId=@CompanyId

-----

set @ResultCount=@@rowcount
";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(c_updateSql, this);

						if (ResultCount == 0)
							throw new OutdatedTimestampApiException("The are no specific company");
					}
				}
			}
		}
	}
}
