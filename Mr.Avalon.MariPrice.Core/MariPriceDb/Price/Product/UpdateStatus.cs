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
			public partial class Product
			{
				[BindStruct]
				public class UpdateStatus
				{
					[NVarChar("ProductUid", 200)]
					public string ProductUid { get; set; }
					[Bind("Status")]
					public int Status { get; set; }

					#region updateSql

					const string c_updateSql = @"
						update [MariPrice].[Product] 
							SET Status = @Status  
						WHERE ProductUid = @ProductUid";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						var query = c_updateSql;
						sql.Query(query, this);
					}
				}
			}
		}
	}
}
