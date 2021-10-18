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
				public class ProductClusterDelete
				{
					[Bind("PriceClusterId")]
					public int PriceClusterId { get; set; }

					#region updateSql

					const string c_updateSql = @"
DELETE pc
FROM [MariPrice].[ProductCluster] pc
WHERE 
	pc.[PriceClusterId] = @PriceClusterId

";

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
