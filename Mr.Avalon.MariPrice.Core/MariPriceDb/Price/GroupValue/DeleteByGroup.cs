using System;
using System.Collections.Generic;
using System.Text;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			public partial class GroupValue
			{
				[BindStruct]
				public class DeleteByGroup
				{
					[Bind("PriceGroupId")]
					public int PriceGroupId { get; set; }

					#region insertSql

					const string c_insertSql = @"
DELETE FROM  [MariPrice].[PriceGroupValue] WHERE	[PriceGroupId]=@PriceGroupId
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
